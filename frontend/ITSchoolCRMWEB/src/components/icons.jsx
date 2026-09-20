// Набор иконок в стиле ДС Ростелеком: контурные (outline), базовая сетка 24×24,
// stroke currentColor — наследуют цвет текста/кнопки. Без внешних зависимостей.
// Иконки можно ресайзить пропом size (от 16 до 32).

function Svg({ size = 24, children }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      {children}
    </svg>
  )
}

// Основное
export function IconHome({ size }) {
  return (
    <Svg size={size}>
      <path d="M3 10.5 12 3l9 7.5" />
      <path d="M5 9.5V21h14V9.5" />
      <path d="M9.5 21v-6h5v6" />
    </Svg>
  )
}

export function IconSwap({ size }) {
  return (
    <Svg size={size}>
      <path d="M7 4 3 8l4 4" />
      <path d="M3 8h13" />
      <path d="m17 12 4 4-4 4" />
      <path d="M21 16H8" />
    </Svg>
  )
}

// Каталоги
export function IconBuilding({ size }) {
  return (
    <Svg size={size}>
      <path d="M4 21V5a1 1 0 0 1 1-1h9a1 1 0 0 1 1 1v16" />
      <path d="M15 9h4a1 1 0 0 1 1 1v11" />
      <path d="M2 21h20" />
      <path d="M8 8h2" />
      <path d="M8 12h2" />
      <path d="M8 16h2" />
      <path d="M12 8h1" />
      <path d="M12 12h1" />
    </Svg>
  )
}

export function IconLayers({ size }) {
  return (
    <Svg size={size}>
      <path d="m12 3 9 5-9 5-9-5 9-5Z" />
      <path d="m3 13 9 5 9-5" />
    </Svg>
  )
}

export function IconUsers({ size }) {
  return (
    <Svg size={size}>
      <circle cx="9" cy="8" r="3.5" />
      <path d="M2.5 20c.8-3.2 3.4-5 6.5-5s5.7 1.8 6.5 5" />
      <path d="M16 4.6a3.5 3.5 0 0 1 0 6.8" />
      <path d="M18.5 15.4c1.6.7 2.7 2.2 3 4.6" />
    </Svg>
  )
}

export function IconDoc({ size }) {
  return (
    <Svg size={size}>
      <path d="M6 3h8l4 4v14H6V3Z" />
      <path d="M14 3v4h4" />
      <path d="M9 12h6" />
      <path d="M9 16h6" />
    </Svg>
  )
}

// Работа
export function IconChart({ size }) {
  return (
    <Svg size={size}>
      <path d="M4 20V4" />
      <path d="M4 20h16" />
      <path d="M8 16v-5" />
      <path d="M12 16V8" />
      <path d="M16 16v-3" />
    </Svg>
  )
}

export function IconPie({ size }) {
  return (
    <Svg size={size}>
      <path d="M21 12a9 9 0 1 1-9-9" />
      <path d="M12 3v9h9" />
      <path d="M15.5 3.5A9 9 0 0 1 20.5 8.5" />
    </Svg>
  )
}

// Справка / Система
export function IconBook({ size }) {
  return (
    <Svg size={size}>
      <path d="M4 5a2 2 0 0 1 2-2h13v16H6a2 2 0 0 0-2 2V5Z" />
      <path d="M4 19a2 2 0 0 1 2-2h13" />
      <path d="M8 7h7" />
    </Svg>
  )
}

export function IconGear({ size }) {
  return (
    <Svg size={size}>
      <circle cx="12" cy="12" r="3" />
      <path d="M12 2.8 13 5a7.5 7.5 0 0 1 2.5 1l2.2-1 1.5 2.6-1.6 1.8a7.6 7.6 0 0 1 0 2.2l1.6 1.8-1.5 2.6-2.2-1a7.5 7.5 0 0 1-2.5 1l-1 2.2-3-.1-.9-2.1a7.5 7.5 0 0 1-2.5-1l-2.2 1-1.5-2.6L4.5 13a7.6 7.6 0 0 1 0-2.2L2.9 9l1.5-2.6 2.2 1A7.5 7.5 0 0 1 9 6.5l1-2.4 2-.3Z" />
    </Svg>
  )
}

// Верхняя панель
export function IconSearch({ size }) {
  return (
    <Svg size={size}>
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3.5-3.5" />
    </Svg>
  )
}

export function IconBell({ size }) {
  return (
    <Svg size={size}>
      <path d="M6 9a6 6 0 1 1 12 0c0 5 2 6 2 6H4s2-1 2-6" />
      <path d="M10 20a2 2 0 0 0 4 0" />
    </Svg>
  )
}

export function IconChevronDown({ size = 16 }) {
  return (
    <Svg size={size}>
      <path d="m6 9 6 6 6-6" />
    </Svg>
  )
}

export function IconLogout({ size }) {
  return (
    <Svg size={size}>
      <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
      <path d="m16 17 5-5-5-5" />
      <path d="M21 12H9" />
    </Svg>
  )
}

export function IconDownload({ size = 16 }) {
  return (
    <Svg size={size}>
      <path d="M12 3v12" />
      <path d="m7 10 5 5 5-5" />
      <path d="M4 21h16" />
    </Svg>
  )
}

export function IconReport({ size = 16 }) {
  return (
    <Svg size={size}>
      <path d="M6 3h8l4 4v14H6V3Z" />
      <path d="M14 3v4h4" />
      <path d="m9 15 2 2 4-4" />
    </Svg>
  )
}

export function IconArrowRight({ size = 16 }) {
  return (
    <Svg size={size}>
      <path d="M4 12h16" />
      <path d="m14 6 6 6-6 6" />
    </Svg>
  )
}

export function IconMenu({ size = 22 }) {
  return (
    <Svg size={size}>
      <path d="M4 6h16" />
      <path d="M4 12h16" />
      <path d="M4 18h16" />
    </Svg>
  )
}

// KPI-иконки
export function IconBuildingActive({ size = 22 }) {
  return <IconBuilding size={size} />
}

export function IconClock({ size = 22 }) {
  return (
    <Svg size={size}>
      <circle cx="12" cy="12" r="9" />
      <path d="M12 7v5l3.5 2" />
    </Svg>
  )
}

export function IconLicense({ size = 22 }) {
  return (
    <Svg size={size}>
      <rect x="3" y="4" width="18" height="16" rx="2" />
      <circle cx="12" cy="11" r="3" />
      <path d="M9.5 13.5 8 20l4-2 4 2-1.5-6.5" />
    </Svg>
  )
}

export function IconSignature({ size = 22 }) {
  return (
    <Svg size={size}>
      <path d="M3 17c2-1 3-4 4-4s1 3 2 3 2-6 3-6 1.5 5 3 5 2-3 3-3 1.5 2 3 2" />
      <path d="M3 21h18" />
    </Svg>
  )
}

// Иконки для страницы «Взаимодействия» (добавить в конец файла icons.jsx)

// Рукопожатие — знак этапа workflow (по макету «картинка рукопожатия»)
export function IconHandshake({ size = 22 }) {
  return (
    <Svg size={size}>
      <path d="M8 12 5.5 9.5a2.1 2.1 0 0 1 3-3L11 9" />
      <path d="m11 9 3.5-3.5a2.1 2.1 0 0 1 3 3L14 12" />
      <path d="m8 12 3 3" />
      <path d="m11 9 3 3" />
      <path d="m5.5 13.5 3 3a2.1 2.1 0 0 0 3 0" />
      <path d="m14 12 3.5 3.5" />
      <path d="m9.5 18.5 2 2a2.1 2.1 0 0 0 3-3l-1-1" />
    </Svg>
  )
}

// Галочка — пройденный этап
export function IconCheck({ size = 20 }) {
  return (
    <Svg size={size}>
      <path d="m4.5 12.5 5 5 10-11" />
    </Svg>
  )
}

// Скрепка — вложение
export function IconPaperclip({ size = 16 }) {
  return (
    <Svg size={size}>
      <path d="m20 11.5-8.5 8.5a5 5 0 0 1-7-7L13 4.5a3.4 3.4 0 0 1 4.8 4.8l-8.3 8.3a1.7 1.7 0 0 1-2.4-2.4l7.8-7.8" />
    </Svg>
  )
}

// Загрузка в облако — дропзона файлов
export function IconUpload({ size = 28 }) {
  return (
    <Svg size={size}>
      <path d="M12 16V4" />
      <path d="m7 9 5-5 5 5" />
      <path d="M4 20h16" />
    </Svg>
  )
}

// Плюс — добавление статуса / взаимодействия
export function IconPlus({ size = 16 }) {
  return (
    <Svg size={size}>
      <path d="M12 5v14" />
      <path d="M5 12h14" />
    </Svg>
  )
}

// Карандаш — редактирование
export function IconEdit({ size = 16 }) {
  return (
    <Svg size={size}>
      <path d="M4 20h4l11-11a2.1 2.1 0 0 0-3-3L5 17v3Z" />
      <path d="m13.5 6.5 3 3" />
    </Svg>
  )
}

// Корзина — удаление файла
export function IconTrash({ size = 16 }) {
  return (
    <Svg size={size}>
      <path d="M4 7h16" />
      <path d="M9 7V5a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
      <path d="M6 7l1 13h10l1-13" />
      <path d="M10 11v6" />
      <path d="M14 11v6" />
    </Svg>
  )
}