// Хелперы для отчётов и статистики.
// Отдельные функции (а не общий client.js), потому что здесь нужен
// скачивание файлов (blob) — общий клиент всё парсит как JSON.

const API_URL = import.meta.env.VITE_API_URL ?? '/api'

function authHeaders() {
  const token = localStorage.getItem('access_token')
  return token ? { Authorization: `Bearer ${token}` } : {}
}

// Имя файла из заголовка Content-Disposition бэкенда
function extractFilename(response, fallback) {
  const disposition = response.headers.get('Content-Disposition') ?? ''
  const match = disposition.match(/filename="?([^";]+)"?/)
  return match ? match[1] : fallback
}

// Скачивание готового Blob отдельным файлом
function saveBlob(blob, filename) {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}

// Статистика по фильтру: { totalInteractions, byStatus[], byUniversity[], byDirection[], byProduct[] }
export async function getStatistics(filter) {
  const response = await fetch(`${API_URL}/Reports/statistics`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(filter ?? {}),
  })
  if (!response.ok) {
    throw new Error('Не удалось загрузить статистику')
  }
  return response.json()
}

// Скачивание отчёта в формате format ('xls' | 'xlsx' | 'pdf' | 'json').
// payload — готовое тело запроса (ReportFilterDto): период + фильтры
// (массивы id) + булевы флаги include* с выбранными колонками.
// ИМЕНА ПОЛЕЙ ДОЛЖНЫ СОВПАДАТЬ СО СВОЙСТВАМИ ReportFilterDto на бэкенде —
// иначе десериализатор их молча проигнорирует и отчёт выйдет
// без фильтров (прежний баг: managerIds vs ResponsibleUserIds).
// Бэкенд формирует файл строго по переданным колонкам.
// Имя файла берём из заголовка Content-Disposition бэкенда.
export async function downloadReport(format, payload) {
  const response = await fetch(`${API_URL}/Reports/${format}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(payload ?? {}),
  })

  if (!response.ok) {
    let message = 'Не удалось сформировать отчёт'
    try {
      const errorBody = await response.json()
      if (errorBody?.message) message = errorBody.message
    } catch {
      // тело не JSON — оставляем дефолтное сообщение
    }
    throw new Error(message)
  }

  // ---------- JSON: пересборка файла на клиенте ----------
  // Бэкенд отдаёт компактный JSON с экранированной кириллицей
  // (все русские буквы вида \u0412\u043E…) — формально валидно, но
  // в текстовых редакторах файл выглядит «поломанным», а без BOM
  // Windows-программы могут неверно определить кодировку.
  // Решение: парсим ответ и записываем заново —
  //   • pretty-print (отступ 2) — файл читается человеком;
  //   • кириллица без \uXXXX-экранирования;
  //   • UTF-8 с BOM (\uFEFF) — Notepad/Excel открывают без кракозябр
  //     (замечание заказчика от 16.09.2026 про кодировку файлов).
  if (format === 'json') {
    const data = await response.json()
    const text = JSON.stringify(data, null, 2)
    const blob = new Blob(['\uFEFF' + text], {
      type: 'application/json;charset=utf-8',
    })
    saveBlob(blob, extractFilename(response, `report-${Date.now()}.json`))
    return
  }

  // ---------- Остальные форматы (xls/xlsx/pdf): файл как есть ----------
  const blob = await response.blob()
  saveBlob(blob, extractFilename(response, `report.${format}`))
}