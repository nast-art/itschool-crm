// Хелперы для отчётов и статистики.
// Отдельные функции (а не общий client.js), потому что здесь нужен
// скачивание файлов (blob) — общий клиент всё парсит как JSON.

const API_URL = import.meta.env.VITE_API_URL ?? '/api'

function authHeaders() {
  const token = localStorage.getItem('access_token')
  return token ? { Authorization: `Bearer ${token}` } : {}
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
// Имя файла берём из заголовка Content-Disposition бэкенда.
export async function downloadReport(format, filter) {
  const response = await fetch(`${API_URL}/Reports/${format}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(filter ?? {}),
  })
  if (!response.ok) {
    throw new Error('Не удалось сформировать отчёт')
  }

  const blob = await response.blob()

  const disposition = response.headers.get('Content-Disposition') ?? ''
  const match = disposition.match(/filename="?([^";]+)"?/)
  const filename = match ? match[1] : `report.${format}`

  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}