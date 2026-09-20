// Хелперы для страницы «Вузы».
// Вынесены отдельно: здесь скачивание справочников договоров/лицензий
// и multipart-импорт каталога (требование 1 ТЗ: актуализация каталога
// через подгрузку xls/xlsx по согласованному маппингу полей).

const API_URL = import.meta.env.VITE_API_URL ?? '/api'

function authHeaders() {
  const token = localStorage.getItem('access_token')
  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function parseError(response, fallback) {
  let message = fallback
  try {
    const body = await response.json()
    if (body?.message) message = body.message
  } catch {
    // тело не JSON — оставляем дефолтное сообщение
  }
  return message
}

// Справочник договоров (нужен для колонки «№ Договора»)
export async function getContracts() {
  const response = await fetch(`${API_URL}/Contracts`, { headers: authHeaders() })
  if (!response.ok) {
    throw new Error(await parseError(response, 'Не удалось загрузить договоры'))
  }
  return response.json()
}

// Справочник лицензий (колонки «Подписание лицензии»,
// «Срок действия», «Статус передачи», «Комментарий»)
export async function getLicenses() {
  const response = await fetch(`${API_URL}/Licenses`, { headers: authHeaders() })
  if (!response.ok) {
    throw new Error(await parseError(response, 'Не удалось загрузить лицензии'))
  }
  return response.json()
}

// Импорт каталога вузов: multipart с файлом .xls/.xlsx.
// Маппинг полей файла — согласованный (требование 1 ТЗ):
//   Название ВУЗа · Вендор · ПО · Номер договора · Подписание лицензии ·
//   Срок действия лицензии (год) · Статус по передачи · ФИО Менеджера ·
//   Ответственные от ВУЗа · Комментарий
export async function importUniversityCatalog(file) {
  const form = new FormData()
  form.append('file', file)

  // Content-Type НЕ выставляем вручную — браузер сам поставит boundary
  const response = await fetch(`${API_URL}/Universities/import`, {
    method: 'POST',
    headers: authHeaders(),
    body: form,
  })

  if (!response.ok) {
    throw new Error(await parseError(response, 'Не удалось импортировать каталог'))
  }

  return response.status === 204 ? null : response.json()
}