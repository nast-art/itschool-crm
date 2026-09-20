// Единый HTTP-клиент для обращений к бэкенду ITSchoolCRM.API.
// Автоматически прикрепляет JWT, при 401 молча обновляет пару токенов
// через /api/Auth/refresh (refresh token хранится в localStorage) и
// повторяет исходный запрос. Разбирает ErrorResponseDto ({ code, message }).

const API_URL = import.meta.env.VITE_API_URL ?? '/api'

// Эндпоинты авторизации: для них 401 — это осмысленный ответ (неверный
// логин/пароль, протухшая сессия), обновлять токен при таком 401 не нужно.
// ВАЖНО: /Auth/me сюда НЕ входит — он защищённый и как раз требует рефреша.
const AUTH_ENDPOINTS = ['/Auth/login', '/Auth/register', '/Auth/refresh']

export class ApiError extends Error {
  constructor(code, message, status) {
    super(message)
    this.code = code
    this.status = status
  }
}

// Одна общая «полётная» задача на обновление, чтобы параллельные запросы
// не дёргали /refresh десять раз одновременно.
let refreshPromise = null

async function tryRefreshTokens() {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      const refreshToken = localStorage.getItem('refresh_token')
      if (!refreshToken) return false

      try {
        const response = await fetch(`${API_URL}/Auth/refresh`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken }),
        })

        if (!response.ok) return false

        const data = await response.json()
        localStorage.setItem('access_token', data.accessToken)
        if (data.refreshToken) {
          localStorage.setItem('refresh_token', data.refreshToken)
        }
        return true
      } catch {
        return false
      }
    })().finally(() => {
      refreshPromise = null
    })
  }

  return refreshPromise
}

function redirectToLogin() {
  localStorage.removeItem('access_token')
  localStorage.removeItem('refresh_token')
  if (!window.location.pathname.startsWith('/login')) {
    window.location.href = '/login'
  }
}

async function request(path, options = {}, allowRefresh = true) {
  const token = localStorage.getItem('access_token')

  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers ?? {}),
  }

  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }

  let response
  try {
    response = await fetch(`${API_URL}${path}`, { ...options, headers })
  } catch {
    throw new ApiError('ERR_NETWORK', 'Сервер недоступен. Проверьте, что API запущен.', 0)
  }

  if (response.status === 401) {
    // Сначала пробуем обновить токен и повторить запрос — для всех
    // защищённых эндпоинтов, включая /Auth/me.
    if (allowRefresh && !AUTH_ENDPOINTS.some((p) => path.startsWith(p))) {
      const refreshed = await tryRefreshTokens()
      if (refreshed) {
        return request(path, options, false)
      }
    }

    // Токен не обновился: читаем тело — сервер мог прислать осмысленную
    // ошибку (ERR_INVALID_CREDENTIALS, ERR_SESSION_EXPIRED).
    let body = null
    try {
      body = await response.json()
    } catch {
      // тело не JSON
    }

    if (body?.code && body.code !== 'ERR_SESSION_EXPIRED') {
      throw new ApiError(body.code, body.message ?? 'Ошибка авторизации', 401)
    }

    redirectToLogin()
    throw new ApiError(
      body?.code ?? 'ERR_UNAUTHORIZED',
      body?.message ?? 'Требуется авторизация',
      401,
    )
  }

  if (!response.ok) {
    let body = null
    try {
      body = await response.json()
    } catch {
      // тело не JSON
    }
    throw new ApiError(
      body?.code ?? 'ERR_INTERNAL',
      body?.message ?? `Ошибка сервера (${response.status})`,
      response.status,
    )
  }

  if (response.status === 204) return null
  return response.json()
}

export const api = {
  get: (path) => request(path),
  post: (path, body) => request(path, { method: 'POST', body: JSON.stringify(body ?? {}) }),
  put: (path, body) => request(path, { method: 'PUT', body: JSON.stringify(body ?? {}) }),
  del: (path) => request(path, { method: 'DELETE' }),
}