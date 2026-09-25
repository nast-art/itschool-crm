import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { notificationService } from '../api/notifications.js'
import { logout } from '../auth/auth'
import { useAuth } from '../App.jsx'
import {
  IconBell,
  IconSearch,
  IconChevronDown,
  IconLogout,
  IconMenu,
} from './icons.jsx'

// Хук: закрыть выпадашку по клику вне её.
function useClickOutside(ref, onClose) {
  useEffect(() => {
    function handler(event) {
      if (ref.current && !ref.current.contains(event.target)) {
        onClose()
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [ref, onClose])
}

// Подписи типов уведомлений — тип приходит с бэкенда
// (stale_status, license_expiring), в интерфейсе — человекочитаемая метка
const NOTIFICATION_TYPE_LABELS = {
  stale_status: 'Зависшая заявка',
  license_expiring: 'Лицензия',
  integration: 'Интеграция',
  system: 'Система',
}

// «сегодня, 14:32» / «21.09.2026» — короткая подпись ко времени уведомления
function formatWhen(iso) {
  if (!iso) return ''
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return ''
  const today = new Date()
  const sameDay = d.toDateString() === today.toDateString()
  return sameDay
    ? `сегодня, ${d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })}`
    : d.toLocaleDateString('ru-RU')
}

export default function Topbar({ title }) {
  const navigate = useNavigate()
  const { user } = useAuth()

  const [query, setQuery] = useState('')
  const [universities, setUniversities] = useState([])
  const [searchOpen, setSearchOpen] = useState(false)
  const [bellOpen, setBellOpen] = useState(false)
  const [profileOpen, setProfileOpen] = useState(false)

  const searchRef = useRef(null)
  const bellRef = useRef(null)
  const profileRef = useRef(null)

  useClickOutside(searchRef, () => setSearchOpen(false))
  useClickOutside(bellRef, () => setBellOpen(false))
  useClickOutside(profileRef, () => setProfileOpen(false))

  // Подписка на NotificationService: снапшот приходит синхронно,
  // дальше сервис сам обновляет (поллинг 30 с + возврат во вкладку).
  const [snapshot, setSnapshot] = useState(() => notificationService.getSnapshot())
  useEffect(() => notificationService.subscribe(setSnapshot), [])
  const { notifications, unreadCount, loaded, error } = snapshot

  // Справочник вузов для поиска
  useEffect(() => {
    api.get('/Universities')
      .then((list) => setUniversities(list ?? []))
      .catch(() => {})
  }, [])

  const trimmed = query.trim().toLowerCase()
  const results = trimmed
    ? universities
        .filter((u) => (u.name ?? '').toLowerCase().includes(trimmed))
        .slice(0, 8)
    : []

  const displayName = user?.fullName || user?.userName || user?.email || 'Пользователь'
  const initials = displayName.trim().charAt(0).toUpperCase()

  // Открытие панели уведомлений = прочитали их.
  // Сервис обновляет состояние оптимистично и догоняет бэкенд
  // (POST /Notifications/read-all).
  function handleBellClick() {
    const willOpen = !bellOpen
    setBellOpen(willOpen)
    setProfileOpen(false)
    if (willOpen) {
      notificationService.markAllRead()
    }
  }

  // Клик по уведомлению с link: пометка прочитанным + переход
  function handleNotificationClick(n) {
    if (!n.isRead) {
      notificationService.markRead(n.id)
    }
    if (n.link) {
      setBellOpen(false)
      navigate(n.link)
    }
  }

  // Клик по результату поиска: переход в реестр «Вузы» С ФИЛЬТРОМ
  // по выбранному вузу (?university={id}). Работает с любой страницы:
  // страница «Вузы» принимает параметр и применяет фильтр сама
  // (функциональное требование 1 ТЗ — фильтрация по выбранным вузам).
  function handleUniversityClick(u) {
    setQuery('')
    setSearchOpen(false)
    navigate(`/universities?university=${u.id}`)
  }

  return (
    <header className="topbar">
      {/* Гамбургер для мобильной версии (открывает боковое меню через CSS) */}
      <label className="topbar-burger" htmlFor="nav-toggle" aria-label="Меню">
        <IconMenu />
      </label>

      <h1 className="topbar-title">{title}</h1>

      <div className="topbar-search" ref={searchRef}>
        <IconSearch size={18} />
        <input
          type="text"
          value={query}
          onChange={(e) => {
            setQuery(e.target.value)
            setSearchOpen(true)
          }}
          onFocus={() => setSearchOpen(true)}
          placeholder="Поиск по вузам…"
        />
        {searchOpen && trimmed && (
          <div className="dropdown">
            {results.length === 0 ? (
              <div className="dropdown-empty">Ничего не найдено</div>
            ) : (
              results.map((u) => (
                <button
                  key={u.id}
                  className="dropdown-item"
                  onClick={() => handleUniversityClick(u)}
                >
                  {u.name}
                </button>
              ))
            )}
          </div>
        )}
      </div>

      <div className="topbar-right">
        {/* Уведомления */}
        <div className="topbar-item" ref={bellRef}>
          <button
            className="icon-btn"
            onClick={handleBellClick}
            aria-label="Уведомления"
          >
            <IconBell size={22} />
            {unreadCount > 0 && (
              <span className="icon-badge">{unreadCount}</span>
            )}
          </button>
          <span className="topbar-hint">Уведомления</span>

          {bellOpen && (
            <div className="dropdown dropdown--right dropdown--notifications">
              {/* Шапка панели: заголовок + «Прочитать все» (не скроллится) */}
              <div className="dropdown-title dropdown-title--row">
                <span>Уведомления</span>
                {unreadCount > 0 && (
                  <button
                    type="button"
                    className="notif-readall"
                    onClick={(e) => {
                      e.stopPropagation()
                      notificationService.markAllRead()
                    }}
                  >
                    Прочитать все
                  </button>
                )}
              </div>

              {/* Список — единственная скроллящаяся часть панели */}
              <div className="dropdown-list">
                {/* Состояния: загрузка / ошибка без кэша / пусто */}
                {!loaded && !error && (
                  <div className="dropdown-empty">Загрузка…</div>
                )}
                {error && !loaded && (
                  <div className="dropdown-empty">
                    Уведомления недоступны. Попробуйте позже.
                  </div>
                )}
                {loaded && notifications.length === 0 && (
                  <div className="dropdown-empty">Уведомлений нет</div>
                )}

                {notifications.map((n) => (
                  <div
                    key={n.id}
                    className={n.isRead ? 'notif' : 'notif is-unread'}
                    role={n.link ? 'button' : undefined}
                    tabIndex={n.link ? 0 : undefined}
                    onClick={() => handleNotificationClick(n)}
                    onKeyDown={(e) => {
                      if (n.link && (e.key === 'Enter' || e.key === ' ')) {
                        e.preventDefault()
                        handleNotificationClick(n)
                      }
                    }}
                  >
                    {/* Служебная строка: метка типа слева, время справа */}
                    <div className="notif-meta">
                      <span className="notif-type">
                        {NOTIFICATION_TYPE_LABELS[n.type] ?? 'Уведомление'}
                      </span>
                      {formatWhen(n.createdAt) && (
                        <span className="notif-time">{formatWhen(n.createdAt)}</span>
                      )}
                    </div>
                    <p className="notif-title">{n.title}</p>
                    {/* Текст — максимум 2 строки; полный по наведению (title) */}
                    <p className="notif-text" title={n.text}>
                      {n.text}
                    </p>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Профиль */}
        <div className="topbar-item" ref={profileRef}>
          <button
            className="profile-btn"
            onClick={() => {
              setProfileOpen((v) => !v)
              setBellOpen(false)
            }}
          >
            <span className="avatar">{initials}</span>
            <span className="profile-name">{displayName}</span>
            <IconChevronDown size={16} />
          </button>
          <span className="topbar-hint">Профиль</span>

          {profileOpen && (
            <div className="dropdown dropdown--right">
              <div className="dropdown-title">{displayName}</div>
              <div className="dropdown-note">
                <p>{user?.email ?? ''}</p>
                <p className="dropdown-roles">
                  Роли: {(user?.roles ?? []).join(', ') || '—'}
                </p>
              </div>
              <button className="dropdown-item danger" onClick={logout}>
                <IconLogout size={16} />
                Выйти
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  )
}