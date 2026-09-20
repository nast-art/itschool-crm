import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { logout } from '../auth/auth'
import { useAuth } from '../App.jsx'
import { requestUniversityFilter } from '../state/dashboardFilter.js'
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

// Демо-уведомления (заглушка до появления реального сервиса нотификаций).
const NOTIFICATIONS = [
  {
    id: 1,
    title: 'Интеграция с LMS',
    text: 'Последняя синхронизация прошла успешно (заглушка).',
    time: 'сегодня',
  },
  {
    id: 2,
    title: 'Напоминание',
    text: 'Проверьте взаимодействия, зависшие в одном статусе более 14 дней.',
    time: 'сегодня',
  },
]

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
                  onClick={() => {
                    setQuery('')
                    setSearchOpen(false)
                    navigate('/universities')
                  }}
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
            onClick={() => {
              setBellOpen((v) => !v)
              setProfileOpen(false)
            }}
            aria-label="Уведомления"
          >
            <IconBell size={22} />
            {NOTIFICATIONS.length > 0 && (
              <span className="icon-badge">{NOTIFICATIONS.length}</span>
            )}
          </button>
          <span className="topbar-hint">Уведомления</span>

          {bellOpen && (
            <div className="dropdown dropdown--right">
              <div className="dropdown-title">Уведомления</div>
              {NOTIFICATIONS.map((n) => (
                <div key={n.id} className="dropdown-note">
                  <strong>{n.title}</strong>
                  <p>{n.text}</p>
                  <span>{n.time}</span>
                </div>
              ))}
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
                <IconLogout size={16} /> Выйти
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  )
}