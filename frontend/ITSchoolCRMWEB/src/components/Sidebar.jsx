import { NavLink } from 'react-router-dom'
import { useAuth } from '../App.jsx'
import {
  IconHome,
  IconSwap,
  IconBuilding,
  IconLayers,
  IconUsers,
  IconDoc,
  IconChart,
  IconPie,
  IconBook,
  IconGear,
} from './icons.jsx'

// Боковое меню приложения.
// Раздел «Система» виден только администраторам (роль admin из Keycloak).
// На мобильных меню управляется чекбоксом #nav-toggle: крестик и клик
// по пункту меню снимают чекбокс — меню закрывается.

const GROUPS = [
  {
    icon: IconHome,
    title: 'Основное',
    items: [
      { to: '/', label: 'Главная', end: true },
      { to: '/interactions', label: 'Взаимодействия' },
    ],
  },
  {
    icon: IconBook,
    title: 'Каталоги',
    items: [
      { to: '/universities', label: 'Вузы' },
      { to: '/directions', label: 'ИТ-направления' },
      { to: '/responsibles', label: 'Ответственные' },
      { to: '/contracts', label: 'Договоры' },
    ],
  },
  {
    icon: IconChart,
    title: 'Работа',
    items: [
      { to: '/reports', label: 'Отчёты' },
      { to: '/visualization', label: 'Визуализация' },
    ],
  },
  {
    icon: IconPie,
    title: 'Справка',
    items: [{ to: '/docs', label: 'Документация' }],
  },
]

const SYSTEM_GROUP = {
  icon: IconGear,
  title: 'Система',
  items: [
    { to: '/system', label: 'Пользователи и права', end: true },
    { to: '/system/workflows', label: 'Workflow' },
    { to: '/system/integrations', label: 'Интеграции API' },
  ],
}

// Снять чекбокс мобильного меню (закрыть его), если он включён
function closeMobileNav() {
  const toggle = document.getElementById('nav-toggle')
  if (toggle) toggle.checked = false
}

function NavItems({ items }) {
  return (
    <ul className="side-list">
      {items.map((item) => (
        <li key={item.to}>
          <NavLink
            to={item.to}
            end={item.end}
            onClick={closeMobileNav}
            className={({ isActive }) => (isActive ? 'side-link active' : 'side-link')}
          >
            {item.label}
          </NavLink>
        </li>
      ))}
    </ul>
  )
}

function Group({ icon: GroupIcon, title, items }) {
  return (
    <div className="side-group">
      <div className="side-group-title">
        <span className="side-group-icon">
          <GroupIcon size={18} />
        </span>
        {title}
      </div>
      <NavItems items={items} />
    </div>
  )
}

export default function Sidebar() {
  const { user } = useAuth()
  const isAdmin = (user?.roles ?? []).includes('admin')

  return (
    <aside className="sidebar">
      <div className="sidebar-logo">IT School CRM</div>

      {/* Крестик закрытия — виден только на мобильных (см. CSS) */}
      <label className="sidebar-close" htmlFor="nav-toggle" aria-label="Закрыть меню">
        ✕
      </label>

      <nav className="sidebar-nav">
        {GROUPS.map((group) => (
          <Group key={group.title} {...group} />
        ))}
        {isAdmin && <Group {...SYSTEM_GROUP} />}
      </nav>

      <div className="sidebar-footer">
        Система контроля обучения <br /> ИТ-направлениям
      </div>
    </aside>
  )
}