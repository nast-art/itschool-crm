// Хелперы для страницы «Взаимодействия».
// Вынесены из общего client.js, потому что здесь нужен multipart-аплоад
// файлов и скачивание бинарных вложений — общий клиент всё парсит как JSON.
//
// Маршрты соответствуют AttachmentsController:
//   POST /Attachments/interaction/{interactionId}/status/{statusId}
//   GET  /Attachments/interaction/{interactionId}
//   GET  /Attachments/{id}/download

const API_URL = import.meta.env.VITE_API_URL ?? '/api'

function authHeaders() {
  const token = localStorage.getItem('access_token')
  return token ? { Authorization: `Bearer ${token}` } : {}
}

// Список вложений взаимодействия
export async function getAttachments(interactionId) {
  const response = await fetch(
    `${API_URL}/Attachments/interaction/${interactionId}`,
    { headers: authHeaders() },
  )
  if (!response.ok) {
    throw new Error('Не удалось загрузить список файлов')
  }
  return response.json()
}

// Загрузка вложения. statusId обязателен (на бэкенде часть маршрута).
export async function uploadAttachment(interactionId, file, statusId) {
  if (statusId == null) {
    throw new Error('Выберите этап, к которому привязывается файл.')
  }

  const form = new FormData()
  form.append('file', file)

  // Content-Type НЕ выставляем вручную — браузер сам поставит boundary
  const response = await fetch(
    `${API_URL}/Attachments/interaction/${interactionId}/status/${statusId}`,
    {
      method: 'POST',
      headers: authHeaders(),
      body: form,
    },
  )

  if (!response.ok) {
    let message = 'Не удалось загрузить файл'
    try {
      const body = await response.json()
      if (body?.message) message = body.message
    } catch {
      // тело не JSON — оставляем дефолтное сообщение
    }
    throw new Error(message)
  }

  return response.json()
}

// Скачивание ранее загруженного вложения
export async function downloadAttachment(attachment) {
  const response = await fetch(
    `${API_URL}/Attachments/${attachment.id}/download`,
    { headers: authHeaders() },
  )

  if (!response.ok) {
    throw new Error('Не удалось скачать файл')
  }

  const blob = await response.blob()

  const disposition = response.headers.get('Content-Disposition') ?? ''
  const match = disposition.match(/filename\*?="?([^";]+)"?/)
  const filename = match ? decodeURIComponent(match[1]) : attachment.fileName ?? 'file'

  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}