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

// Импорт каталога вузов: POST /api/Imports/excel (ImportsController).
// multipart/form-data: file + mapping (строка с JSON ImportMappingDto).
// Маппинг: «колонка Excel → английское имя поля» (ключи MappingFields
// ImportService: UniversityName, ContractNumber и т.д.). Бэкенд проверяет
// mapping.Value через MappingFields.ContainsKey — значениями должны быть
// именно английские ключи, русские названия — это подписи для UI.
const DEFAULT_IMPORT_MAPPING = {
  'Название ВУЗа': 'UniversityName',
  'ИТ-направление': 'DirectionName',
  'ИТ-продукт': 'ProductName',
  'Вендор': 'Vendor',
  'ПО': 'Software',
  'Номер договора': 'ContractNumber',
  'Подписание лицензии': 'LicenseSignedAt',
  'Срок действия лицензии': 'LicenseValidUntil',
  'Статус по передачи': 'TransferStatus',
  'ФИО Менеджера': 'ManagerFullName',
  'Ответственные от ВУЗа': 'UniversityContactFullName',
  'Комментарий': 'Comment',
}

export async function importUniversityCatalog(file) {
  const form = new FormData()
  form.append('file', file)
  form.append('mapping', JSON.stringify({ Mapping: DEFAULT_IMPORT_MAPPING }))

  // Content-Type НЕ выставляем вручную — браузер сам поставит boundary
  const response = await fetch(`${API_URL}/Imports/excel`, {
    method: 'POST',
    headers: authHeaders(),
    body: form,
  })

  if (!response.ok) {
    // Контроллер отдаёт BadRequest строкой, а не { message } — учтём оба варианта
    let message = 'Не удалось импортировать каталог'
    try {
      const text = await response.text()
      try {
        const body = JSON.parse(text)
        message = body?.message ?? body?.title ?? (typeof body === 'string' ? body : message)
      } catch {
        if (text) message = text
      }
    } catch {
      // тело не читается — оставляем дефолт
    }
    throw new Error(message)
  }

  return response.json()
}