// Авторизация и регистрация — весь обмен идёт через НАШ бэкенд:
//   POST /api/Auth/register — регистрация (бэкенд сам работает с Keycloak
//                             Admin API и назначает роль "user")
//   POST /api/Auth/login    — обмен логина/пароля на JWT Keycloak
// Секреты Keycloak на фронтенде отсутствуют полностью.

import { api } from '../api/client'

// Регистрация: ФИО тремя частями (Фамилия / Имя / Отчество),
// отчество необязательно. Роль нигде не выбирается, назначается "user"
// на бэкенде. Ошибки приходят в ErrorResponseDto (ERR_VALIDATION 400,
// ERR_CONFLICT 409, KEYCLOAK_* 502) — показываем message как есть.
export async function register({ lastName, firstName, middleName, email, password }) {
  await api.post('/Auth/register', {
    lastName,
    firstName,
    middleName: middleName || null,
    email,
    password,
  })
}

// Вход: возвращает { accessToken, refreshToken, expiresIn }.
// 401 ERR_INVALID_CREDENTIALS — message: "Неверный логин или пароль."
export async function login(userName, password) {
  const data = await api.post('/Auth/login', { userName, password })
  localStorage.setItem('access_token', data.accessToken)
  localStorage.setItem('refresh_token', data.refreshToken)
  return data
}

export function logout() {
  localStorage.removeItem('access_token')
  localStorage.removeItem('refresh_token')
  window.location.href = '/login'
}

export function isAuthenticated() {
  return Boolean(localStorage.getItem('access_token'))
}

// Роли достаём из payload JWT (realm_access.roles), не делая лишних запросов.
export function getRolesFromToken() {
  const token = localStorage.getItem('access_token')
  if (!token) return []
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload?.realm_access?.roles ?? []
  } catch {
    return []
  }
}