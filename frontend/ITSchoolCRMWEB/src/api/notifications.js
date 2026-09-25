// Уведомления: API-слой + сервис-подписка в одном модуле.
//
// Контракт с бэкендом (ITSchoolCRM.API, NotificationsController):
//   GET  /Notifications            -> [NotificationDto]
//   POST /Notifications/{id}/read  — отметить одно прочитанным
//   POST /Notifications/read-all   — отметить все прочитанными
//
// NotificationDto: { id: string, type, title, text, createdAt, isRead, link }
// Типы: stale_status (зависшее >14 дней), license_expiring (≤30 дней).
//
// ПРОЧИТАННОСТЬ — В LOCALSTORAGE, НЕ В БД (нефункциональное требование 13
// ТЗ: кэш действий пользователя). Уведомления сами по себе в БД не
// хранятся — каждый раз вычисляются из данных CRM (зависшие заявки,
// истекающие лицензии), поэтому флажок «прочитано» — персональный
// UI-кэш, привязанный к браузеру. Ключ кэша включает sub из JWT,
// поэтому разные пользователи на одном компьютере не мешают друг
// другу. Ограничение: на другом устройстве уведомления снова будут
// непрочитанными. POST-ы на бэкенд уходят как и раньше (на случай,
// если там будет реальная персистентность), но при их отсутствии
// сервис работает полностью автономно.

import { api } from './client'

// ---------- API ----------

function normalize(n) {
  return {
    id: String(n.id),
    type: n.type ?? 'system',
    title: n.title ?? 'Уведомление',
    text: n.text ?? n.message ?? '',
    createdAt: n.createdAt ?? n.created_at ?? null,
    isRead: Boolean(n.isRead ?? n.is_read ?? false),
    link: n.link ?? null,
  }
}

async function getNotifications() {
  const list = await api.get('/Notifications')
  return (list ?? [])
    .map(normalize)
    .sort((a, b) => new Date(b.createdAt ?? 0) - new Date(a.createdAt ?? 0))
}

async function markNotificationRead(id) {
  await api.post(`/Notifications/${id}/read`)
}

async function markAllNotificationsRead() {
  await api.post('/Notifications/read-all')
}

// ---------- Кэш прочитанности в localStorage ----------

// Субъект JWT ("sub") = keycloak_user_id пользователя. Ключ кэша
// персональный, чтобы на общем компьютере пользователи не видели
// чужие прочитанные уведомления.
function currentUserKey() {
  try {
    const token = localStorage.getItem('access_token')
    if (!token) return 'anonymous'
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload?.sub ?? 'anonymous'
  } catch {
    return 'anonymous'
  }
}

function storageKey() {
  return `notifications_read_${currentUserKey()}`
}

// Максимум хранимых id — старые обрезаем, чтобы localStorage
// не разрастался бесконечно
const MAX_READ_IDS = 300

function loadReadSet() {
  try {
    const raw = localStorage.getItem(storageKey())
    const parsed = raw ? JSON.parse(raw) : []
    return new Set(Array.isArray(parsed) ? parsed : [])
  } catch {
    return new Set()
  }
}

function saveReadSet(set) {
  const ids = [...set].slice(-MAX_READ_IDS)
  localStorage.setItem(storageKey(), JSON.stringify(ids))
}

// ---------- Сервис ----------

const POLL_INTERVAL_MS = 30_000

class NotificationService {
  constructor() {
    this.listeners = new Set()
    this.notifications = []
    this.loaded = false
    this.error = null
    this.timer = null
    this.subscribers = 0
    this.inFlight = null
    this.handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') this.refresh()
    }
  }

  getSnapshot() {
    return {
      notifications: this.notifications,
      unreadCount: this.notifications.filter((n) => !n.isRead).length,
      loaded: this.loaded,
      error: this.error,
    }
  }

  emit() {
    const snapshot = this.getSnapshot()
    this.listeners.forEach((listener) => listener(snapshot))
  }

  // Подписаться на снапшот; вернуть функцию отписки.
  // Поллинг крутится, пока жив хотя бы один подписчик.
  subscribe(listener) {
    this.listeners.add(listener)
    this.subscribers += 1
    if (this.subscribers === 1) this.startPolling()
    listener(this.getSnapshot()) // синхронный первый снапшот
    return () => {
      this.listeners.delete(listener)
      this.subscribers = Math.max(0, this.subscribers - 1)
      if (this.subscribers === 0) this.stopPolling()
    }
  }

  startPolling() {
    this.refresh()
    this.timer = window.setInterval(() => this.refresh(), POLL_INTERVAL_MS)
    document.addEventListener('visibilitychange', this.handleVisibilityChange)
  }

  stopPolling() {
    window.clearInterval(this.timer)
    this.timer = null
    document.removeEventListener('visibilitychange', this.handleVisibilityChange)
  }

  async refresh() {
    if (this.inFlight) return this.inFlight
    this.inFlight = (async () => {
      try {
        const list = await getNotifications()
        // Накладываем локальные пометки «прочитано» из кэша:
        // бэкенд может вернуть что угодно, источник истины по
        // прочитанности для этого браузера — localStorage
        const readSet = loadReadSet()
        this.notifications = list.map((n) => ({
          ...n,
          isRead: readSet.has(n.id) || n.isRead,
        }))
        this.error = null
        this.loaded = true
      } catch (err) {
        // Не затираем список при кратковременном сбое сети
        this.error = err.message ?? 'Не удалось загрузить уведомления'
      } finally {
        this.inFlight = null
        this.emit()
      }
    })()
    return this.inFlight
  }

  async markRead(id) {
    // 1) Сразу в локальный кэш и состояние UI — мгновенный отклик
    const readSet = loadReadSet()
    readSet.add(id)
    saveReadSet(readSet)
    this.notifications = this.notifications.map((n) =>
      n.id === id ? { ...n, isRead: true } : n,
    )
    this.emit()
    // 2) Догоняем бэкенд (не критично: при сбое кэш уже записан)
    try {
      await markNotificationRead(id)
    } catch {
      // бэкенд недоступен или не хранит прочитанность — игнорируем
    }
  }

  async markAllRead() {
    if (!this.notifications.some((n) => !n.isRead)) return
    // 1) Все ТЕКУЩИЕ id — в локальный кэш
    const readSet = loadReadSet()
    this.notifications.forEach((n) => readSet.add(n.id))
    saveReadSet(readSet)
    this.notifications = this.notifications.map((n) => ({ ...n, isRead: true }))
    this.emit()
    // 2) Догоняем бэкенд
    try {
      await markAllNotificationsRead()
    } catch {
      // см. markRead
    }
  }
}

export const notificationService = new NotificationService()