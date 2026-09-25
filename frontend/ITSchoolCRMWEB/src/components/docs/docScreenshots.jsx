// Встроенные иллюстрации экранов для руководств (нефункциональное
// требование 5 ТЗ: документация содержит скриншоты элементов
// интерфейса). Реализованы как аккуратные SVG-схемы экранов в палитре
// дизайн-системы Ростелеком — страница документации полностью
// самодостаточна и не требует внешних файлов.
//
// ЗАМЕНА НА РЕАЛЬНЫЕ СКРИНШОТЫ:
//   1. Сделайте снимок экрана (Win+Shift+S / Cmd+Shift+4).
//   2. Сохраните PNG в папку  frontend/public/docs/screenshots/
//      (имена: login.png, dashboard.png, interactions.png и т.д.).
//   3. В DocumentationPage.jsx в массиве глав у блока-иллюстрации
//      укажите  src: '/docs/screenshots/login.png'  — реальное фото
//      заменит схему автоматически. Если файл отсутствует, покажется
//      эта встроенная схема (fallback работает из коробки).

// Общая рамка «окна браузера»: шапка с точками + область контента
function ShotFrame({ title, children }) {
  return (
    <svg
      viewBox="0 0 640 372"
      role="img"
      aria-label={`Иллюстрация: ${title}`}
      className="doc-shot-svg"
    >
      {/* Корпус окна */}
      <rect x="1" y="1" width="638" height="370" rx="14" fill="#ffffff" stroke="#dcdce3" />
      {/* Панель браузера */}
      <rect x="1" y="1" width="638" height="34" rx="14" fill="#f2f2f5" />
      <rect x="1" y="20" width="638" height="15" fill="#f2f2f5" />
      <circle cx="22" cy="17" r="5" fill="#fcaaa4" />
      <circle cx="40" cy="17" r="5" fill="#ffe1a3" />
      <circle cx="58" cy="17" r="5" fill="#a9e9c6" />
      <rect x="84" y="8" width="360" height="18" rx="9" fill="#ffffff" stroke="#dcdce3" />
      <text x="96" y="21" fontSize="10" fill="#70707a" fontFamily="inherit">
        IT School CRM — {title}
      </text>
      {/* Контент */}
      {children}
    </svg>
  )
}

// Хлебная подпись под иллюстрацией (общий компонент подписи)
export function ShotCaption({ x = 320, y = 356, text }) {
  return (
    <text x={x} y={y} fontSize="11" fill="#70707a" textAnchor="middle" fontFamily="inherit">
      {text}
    </text>
  )
}

// ---------- 1. Окно входа ----------
export function ShotLogin() {
  return (
    <ShotFrame title="Вход">
      {/* Левая брендовая панель */}
      <rect x="20" y="48" width="290" height="296" rx="12" fill="#fff0e9" stroke="#ffc9b5" />
      <text x="165" y="100" fontSize="15" fontWeight="700" fill="#1b1b1f" textAnchor="middle">
        Система контроля обучения
      </text>
      <text x="165" y="122" fontSize="15" fontWeight="700" fill="#ff4d14" textAnchor="middle">
        ИТ-направлениям
      </text>
      <rect x="60" y="150" width="52" height="44" rx="8" fill="#ffe0d3" />
      <rect x="139" y="150" width="52" height="44" rx="8" fill="#ffe0d3" />
      <rect x="218" y="150" width="52" height="44" rx="8" fill="#ffe0d3" />
      {/* Кампус */}
      <rect x="120" y="230" width="90" height="70" rx="6" fill="#ffffff" stroke="#ffc9b5" />
      <path d="M112 232 L165 196 L218 232 Z" fill="#ffa285" opacity="0.6" />
      <rect x="205" y="180" width="3" height="20" fill="#ff6b3d" />
      <path d="M208 182 L226 187 L208 192 Z" fill="#ff4d14" />
      {/* Правая форма */}
      <rect x="330" y="48" width="290" height="296" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="475" y="92" fontSize="15" fontWeight="700" fill="#1b1b1f" textAnchor="middle">
        Вход в систему
      </text>
      <text x="360" y="128" fontSize="11" fill="#45454d">Логин/Email</text>
      <rect x="360" y="136" width="230" height="36" rx="10" fill="#ffffff" stroke="#dcdce3" />
      <text x="372" y="159" fontSize="11" fill="#a6a6b0">ivanov@edu-rt.ru</text>
      <text x="360" y="198" fontSize="11" fill="#45454d">Пароль</text>
      <rect x="360" y="206" width="230" height="36" rx="10" fill="#ffffff" stroke="#dcdce3" />
      <text x="372" y="229" fontSize="11" fill="#a6a6b0">••••••••</text>
      <text x="556" y="229" fontSize="10" fill="#ff4d14" textAnchor="end">Показать</text>
      <rect x="360" y="262" width="230" height="40" rx="10" fill="#ff4d14" />
      <text x="475" y="287" fontSize="13" fontWeight="600" fill="#ffffff" textAnchor="middle">
        Войти
      </text>
      <text x="475" y="322" fontSize="11" fill="#70707a" textAnchor="middle">
        Нет аккаунта? Зарегистрироваться
      </text>
    </ShotFrame>
  )
}

// ---------- 2. Главная страница ----------
export function ShotDashboard() {
  return (
    <ShotFrame title="Главная">
      {/* KPI */}
      {[0, 1, 2, 3].map((i) => (
        <g key={i}>
          <rect x={20 + i * 153} y="48" width="143" height="66" rx="12" fill="#ffffff" stroke="#e8e8ee" />
          <circle cx={44 + i * 153} cy="81" r="16" fill="#fff0e9" />
          <rect x={68 + i * 153} y="66" width="80" height="10" rx="5" fill="#e9e9ee" />
          <rect x={68 + i * 153} y="84" width="40" height="14" rx="7" fill="#ff4d14" opacity="0.85" />
        </g>
      ))}
      {/* График заявок */}
      <rect x="20" y="126" width="390" height="212" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <rect x="38" y="142" width="180" height="12" rx="6" fill="#e9e9ee" />
      {[0, 1, 2, 3, 4].map((i) => (
        <rect
          key={i}
          x={60 + i * 62}
          y={300 - 30 - i * 22}
          width="34"
          height={30 + i * 22}
          rx="5"
          fill="#ff4d14"
          opacity={0.55 + i * 0.11}
        />
      ))}
      {/* Круговая диаграмма */}
      <rect x="424" y="126" width="196" height="212" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <circle cx="522" cy="208" r="52" fill="none" stroke="#e9e9ee" strokeWidth="22" />
      <path d="M522 156 A52 52 0 0 1 570 196" fill="none" stroke="#8b31ff" strokeWidth="22" />
      <path d="M570 196 A52 52 0 0 1 546 254" fill="none" stroke="#5b7cfa" strokeWidth="22" />
      <path d="M546 254 A52 52 0 0 1 486 238" fill="none" stroke="#2aa7f0" strokeWidth="22" />
      {[0, 1, 2].map((i) => (
        <g key={i}>
          <circle cx="448" cy={286 + i * 16} r="4" fill={['#8b31ff', '#5b7cfa', '#2aa7f0'][i]} />
          <rect x="458" y={282 + i * 16} width="100" height="8" rx="4" fill="#e9e9ee" />
        </g>
      ))}
    </ShotFrame>
  )
}

// ---------- 3. Взаимодействия: карта workflow ----------
export function ShotWorkflow() {
  return (
    <ShotFrame title="Взаимодействия">
      {/* Фильтры */}
      {[0, 1, 2, 3, 4].map((i) => (
        <g key={i}>
          <rect x={20 + i * 121} y="48" width="111" height="12" rx="6" fill="#e9e9ee" />
          <rect x={20 + i * 121} y="64" width="111" height="34" rx="10" fill="#ffffff" stroke="#dcdce3" />
        </g>
      ))}
      {/* Карточки взаимодействий */}
      {[0, 1, 2].map((i) => (
        <g key={i}>
          <rect x={20 + i * 202} y="112" width="192" height="70" rx="12" fill="#ffffff" stroke={i === 0 ? '#ff4d14' : '#e8e8ee'} strokeWidth={i === 0 ? 2 : 1} />
          <rect x={34 + i * 202} y="126" width="110" height="10" rx="5" fill="#e9e9ee" />
          <rect x={34 + i * 202} y="150" width="70" height="16" rx="8" fill="#f4efff" />
          <rect x="128" y="150" width="66" height="16" rx="8" fill="#e8f5fd" />
        </g>
      ))}
      {/* Карта этапов */}
      <rect x="20" y="196" width="600" height="150" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="38" y="222" fontSize="12" fontWeight="600" fill="#1b1b1f">Взаимодействие с ВУЗом</text>
      {[0, 1, 2, 3, 4].map((i) => (
        <g key={i}>
          <circle cx={70 + i * 118} cy="266" r="16" fill={i < 2 ? '#ff4d14' : i === 2 ? '#fff0e9' : '#f0f0f4'} stroke={i <= 2 ? '#ff4d14' : '#d4d4dd'} strokeWidth="2" />
          {i < 2 && (
            <path d={`M${63 + i * 118} 266 l5 5 l10 -11`} stroke="#ffffff" strokeWidth="2.5" fill="none" strokeLinecap="round" />
          )}
          {i < 4 && (
            <rect x={88 + i * 118} y="264" width="82" height="4" rx="2" fill={i < 2 ? '#ff4d14' : '#d4d4dd'} />
          )}
          <rect x={44 + i * 118} y="292" width="52" height="7" rx="3.5" fill="#e9e9ee" />
        </g>
      ))}
      {/* Прогресс */}
      <rect x="38" y="316" width="460" height="8" rx="4" fill="#f0f0f4" />
      <rect x="38" y="316" width="160" height="8" rx="4" fill="#ff4d14" />
      <text x="600" y="324" fontSize="10" fill="#70707a" textAnchor="end">Пройдено 4/14</text>
    </ShotFrame>
  )
}

// ---------- 4. Перевод статуса ----------
export function ShotTransition() {
  return (
    <ShotFrame title="Перевод статуса">
      <rect x="20" y="48" width="380" height="296" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="38" y="76" fontSize="13" fontWeight="600" fill="#1b1b1f">Перевод статуса</text>
      <text x="38" y="106" fontSize="11" fill="#45454d">Новый статус</text>
      <rect x="38" y="114" width="344" height="38" rx="10" fill="#ffffff" stroke="#dcdce3" />
      <text x="52" y="138" fontSize="11" fill="#45454d">Шаг 4 «Обмен пакетом документов…»</text>
      <text x="38" y="182" fontSize="11" fill="#45454d">Комментарий к переходу</text>
      <rect x="38" y="190" width="344" height="90" rx="10" fill="#ffffff" stroke="#ff4d14" />
      <text x="52" y="214" fontSize="11" fill="#a6a6b0">Передали пакет документов на согласование</text>
      <rect x="38" y="294" width="180" height="40" rx="10" fill="#ff4d14" />
      <text x="128" y="319" fontSize="13" fontWeight="600" fill="#ffffff" textAnchor="middle">Перевести статус</text>
      {/* Подсказка справа */}
      <rect x="416" y="48" width="204" height="120" rx="12" fill="#fff0e9" stroke="#ffc9b5" />
      <text x="432" y="74" fontSize="11" fontWeight="700" fill="#c23509">Как это работает</text>
      <text x="432" y="94" fontSize="10" fill="#45454d">Доступные переходы задаёт</text>
      <text x="432" y="108" fontSize="10" fill="#45454d">workflow: вперёд, назад и</text>
      <text x="432" y="122" fontSize="10" fill="#45454d">по ветвлениям. Комментарий</text>
      <text x="432" y="136" fontSize="10" fill="#45454d">сохранится в истории.</text>
      <rect x="416" y="180" width="204" height="164" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="432" y="206" fontSize="11" fontWeight="700" fill="#1b1b1f">Комментарии</text>
      <circle cx="444" cy="230" r="12" fill="#ffe0d3" />
      <rect x="464" y="220" width="140" height="9" rx="4.5" fill="#e9e9ee" />
      <rect x="464" y="234" width="110" height="8" rx="4" fill="#f2f2f5" />
      <circle cx="444" cy="272" r="12" fill="#ffe0d3" />
      <rect x="464" y="262" width="140" height="9" rx="4.5" fill="#e9e9ee" />
      <rect x="464" y="276" width="90" height="8" rx="4" fill="#f2f2f5" />
    </ShotFrame>
  )
}

// ---------- 5. Файлы ----------
export function ShotFiles() {
  return (
    <ShotFrame title="Файлы">
      <rect x="20" y="48" width="380" height="150" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="38" y="76" fontSize="12" fontWeight="600" fill="#1b1b1f">Файлы</text>
      {/* Дропзона */}
      <rect x="38" y="90" width="344" height="66" rx="12" fill="#fff0e9" stroke="#ff4d14" strokeDasharray="6 5" />
      <text x="210" y="118" fontSize="11" fill="#c23509" textAnchor="middle">Перетащите файл сюда</text>
      <text x="210" y="134" fontSize="9" fill="#70707a" textAnchor="middle">png, jpeg, pdf, zip… до 20 МБ</text>
      {/* Список файлов */}
      <rect x="38" y="166" width="344" height="22" rx="6" fill="#f2f2f5" />
      <text x="50" y="181" fontSize="9" fill="#45454d">договор_МГУ.pdf · Шаг 4 · 12.09.2026</text>
      <text x="366" y="181" fontSize="9" fill="#ff4d14" textAnchor="end">Скачать ✕</text>
      {/* Комментарии */}
      <rect x="416" y="48" width="204" height="296" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="432" y="76" fontSize="12" fontWeight="600" fill="#1b1b1f">Комментарии</text>
      {[0, 1, 2].map((i) => (
        <g key={i}>
          <circle cx="444" cy={104 + i * 62} r="12" fill="#ffe0d3" />
          <rect x="464" y={92 + i * 62} width="120" height="9" rx="4.5" fill="#e9e9ee" />
          <rect x="464" y={106 + i * 62} width="140" height="24" rx="6" fill="#f2f2f5" />
        </g>
      ))}
      <rect x="432" y="288" width="172" height="40" rx="10" fill="#fff0e9" stroke="#ffc9b5" />
      <text x="446" y="312" fontSize="10" fill="#a6a6b0">Написать комментарий…</text>
    </ShotFrame>
  )
}

// ---------- 6. Таблица вузов ----------
export function ShotUniversities() {
  return (
    <ShotFrame title="Вузы">
      {/* Фильтры */}
      {[0, 1, 2, 3].map((i) => (
        <g key={i}>
          <rect x={20 + i * 128} y="48" width="118" height="12" rx="6" fill="#e9e9ee" />
          <rect x={20 + i * 128} y="64" width="118" height="32" rx="10" fill="#ffffff" stroke="#dcdce3" />
        </g>
      ))}
      <rect x="532" y="58" width="88" height="38" rx="10" fill="#ff4d14" />
      <text x="576" y="82" fontSize="12" fontWeight="600" fill="#ffffff" textAnchor="middle">Применить</text>
      {/* Таблица */}
      <rect x="20" y="112" width="600" height="200" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      {[0, 1, 2, 3].map((i) => (
        <g key={i}>
          <rect x="20" y={126 + i * 46} width="600" height="1" fill="#e8e8ee" />
          <rect x="34" y={138 + i * 46} width="120" height="10" rx="5" fill={i === 0 ? '#d9d9e0' : '#e9e9ee'} />
          <rect x="196" y={138 + i * 46} width="60" height="10" rx="5" fill="#e9e9ee" />
          <rect x="300" y={138 + i * 46} width="70" height="10" rx="5" fill="#e9e9ee" />
          <rect x="430" y={136 + i * 46} width="86" height="16" rx="8" fill={i === 1 ? '#ecfdf3' : i === 2 ? '#e8f5fd' : '#fff6e0'} />
          <circle cx="592" cy={143 + i * 46} r="10" fill="#fff0e9" />
        </g>
      ))}
      {/* Пагинация */}
      <text x="20" y="336" fontSize="10" fill="#70707a">Показано 1–5 из 160</text>
      <rect x="470" y="322" width="150" height="22" rx="6" fill="#ffffff" stroke="#dcdce3" />
      <text x="545" y="337" fontSize="10" fill="#45454d" textAnchor="middle">‹ Страница 1 из 32 ›</text>
    </ShotFrame>
  )
}

// ---------- 7. Назначение менеджера (руководитель) ----------
export function ShotAssignManager() {
  return (
    <ShotFrame title="Назначить менеджера">
      <rect x="170" y="70" width="300" height="230" rx="16" fill="#ffffff" stroke="#e8e8ee" />
      <text x="320" y="104" fontSize="14" fontWeight="700" fill="#1b1b1f" textAnchor="middle">
        Назначить менеджера
      </text>
      <text x="320" y="126" fontSize="10" fill="#70707a" textAnchor="middle">
        Вуз: МГУ · Python-разработчик
      </text>
      <text x="196" y="162" fontSize="11" fill="#45454d">Менеджер</text>
      <rect x="196" y="170" width="248" height="38" rx="10" fill="#ffffff" stroke="#ff4d14" />
      <text x="210" y="194" fontSize="11" fill="#45454d">Иванова Анна Сергеевна</text>
      <rect x="196" y="228" width="116" height="36" rx="10" fill="#ffffff" stroke="#dcdce3" />
      <text x="254" y="251" fontSize="12" fill="#45454d" textAnchor="middle">Отмена</text>
      <rect x="328" y="228" width="116" height="36" rx="10" fill="#ff4d14" />
      <text x="386" y="251" fontSize="12" fontWeight="600" fill="#ffffff" textAnchor="middle">Назначить</text>
    </ShotFrame>
  )
}

// ---------- 8. ИТ-направления ----------
export function ShotDirections() {
  return (
    <ShotFrame title="ИТ-направления">
      {[0, 1, 2].map((i) => (
        <g key={i}>
          <rect x={20 + i * 202} y="48" width="192" height="140" rx="12" fill="#ffffff" stroke="#e8e8ee" />
          <rect x={34 + i * 202} y="64" width="100" height="12" rx="6" fill="#e9e9ee" />
          <rect x={34 + i * 202} y="88" width="164" height="8" rx="4" fill="#f2f2f5" />
          <rect x={34 + i * 202} y="102" width="130" height="8" rx="4" fill="#f2f2f5" />
          <circle cx="42" cy="132" r="5" fill="#ff4d14" />
          <rect x="52" y="127" width="52" height="9" rx="4.5" fill="#e9e9ee" />
          <rect x="34" y="152" width="120" height="24" rx="8" fill="#fff0e9" stroke="#ffc9b5" />
          <text x="94" y="168" fontSize="9" fill="#c23509" textAnchor="middle">Открыть workflow →</text>
        </g>
      ))}
      {/* Карточка добавления */}
      <rect x="20" y="204" width="192" height="120" rx="12" fill="#ffffff" stroke="#ffc9b5" strokeDasharray="6 5" />
      <circle cx="116" cy="244" r="16" fill="#fff0e9" />
      <text x="116" y="250" fontSize="16" fill="#ff4d14" textAnchor="middle">+</text>
      <text x="116" y="282" fontSize="10" fill="#45454d" textAnchor="middle">Добавить программу</text>
      <text x="116" y="298" fontSize="8" fill="#70707a" textAnchor="middle">Новое ИТ-направление или продукт</text>
      {/* Импорт */}
      <rect x="230" y="204" width="390" height="120" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="250" y="232" fontSize="11" fontWeight="600" fill="#1b1b1f">Каталоги актуализируются через XLS/XLSX</text>
      <text x="250" y="252" fontSize="10" fill="#70707a">Маппинг полей по требованию 1 ТЗ: вуз, вендор, ПО, № договора,</text>
      <text x="250" y="266" fontSize="10" fill="#70707a">подписание и срок лицензии, статус передачи, менеджер,</text>
      <text x="250" y="280" fontSize="10" fill="#70707a">ответственные от вуза, комментарий.</text>
      <rect x="250" y="294" width="130" height="20" rx="6" fill="#fff0e9" />
      <text x="315" y="308" fontSize="9" fill="#c23509" textAnchor="middle">Импорт · руководитель</text>
    </ShotFrame>
  )
}

// ---------- 9. Отчёты ----------
export function ShotReports() {
  return (
    <ShotFrame title="Отчёты">
      {/* Фильтры */}
      {[0, 1, 2, 3, 4].map((i) => (
        <g key={i}>
          <rect x={20 + i * 121} y="48" width="111" height="11" rx="5.5" fill="#e9e9ee" />
          <rect x={20 + i * 121} y="63" width="111" height="30" rx="10" fill="#ffffff" stroke="#dcdce3" />
        </g>
      ))}
      {/* Колонки */}
      <rect x="20" y="106" width="600" height="56" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="36" y="128" fontSize="11" fontWeight="600" fill="#1b1b1f">Выбор колонок отчёта</text>
      {['Вуз', 'Направление', 'Продукт', 'Статус', 'Ответственный'].map((label, i) => (
        <g key={label}>
          <rect x={36 + i * 108} y="138" width="12" height="12" rx="3" fill="#ff4d14" />
          <text x={52 + i * 108} y="148" fontSize="9" fill="#45454d">{label}</text>
        </g>
      ))}
      {/* Таблица предпросмотра */}
      <rect x="20" y="176" width="600" height="168" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      {[0, 1, 2, 3].map((i) => (
        <g key={i}>
          <rect x="20" y={190 + i * 36} width="600" height="1" fill="#e8e8ee" />
          <rect x="34" y={200 + i * 36} width="110" height="9" rx="4.5" fill={i === 0 ? '#d9d9e0' : '#e9e9ee'} />
          <rect x="190" y={200 + i * 36} width="80" height="9" rx="4.5" fill="#e9e9ee" />
          <rect x="300" y={198 + i * 36} width="76" height="15" rx="7.5" fill="#f4efff" />
          <rect x="420" y={200 + i * 36} width="90" height="9" rx="4.5" fill="#e9e9ee" />
        </g>
      ))}
      {/* Экспорт */}
      <rect x="356" y="324" width="78" height="24" rx="8" fill="#fff0e9" stroke="#ffc9b5" />
      <text x="395" y="340" fontSize="9" fill="#c23509" textAnchor="middle">Скачать XLS</text>
      <rect x="442" y="324" width="84" height="24" rx="8" fill="#fff0e9" stroke="#ffc9b5" />
      <text x="484" y="340" fontSize="9" fill="#c23509" textAnchor="middle">Скачать XLSX</text>
      <rect x="534" y="324" width="72" height="24" rx="8" fill="#fff0e9" stroke="#ffc9b5" />
      <text x="570" y="340" fontSize="9" fill="#c23509" textAnchor="middle">Скачать PDF</text>
    </ShotFrame>
  )
}

// ---------- 10. Визуализация ----------
export function ShotVisualization() {
  return (
    <ShotFrame title="Визуализация">
      <rect x="20" y="48" width="295" height="140" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <rect x="36" y="62" width="150" height="10" rx="5" fill="#e9e9ee" />
      {[0, 1, 2, 3].map((i) => (
        <rect key={i} x={56 + i * 60} y={160 - i * 18} width="30" height={20 + i * 18} rx="4" fill="#ff4d14" opacity={0.5 + i * 0.15} />
      ))}
      <rect x="331" y="48" width="289" height="140" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <circle cx="475" cy="118" r="44" fill="none" stroke="#e9e9ee" strokeWidth="18" />
      <path d="M475 74 A44 44 0 0 1 517 104" fill="none" stroke="#8b31ff" strokeWidth="18" />
      <path d="M517 104 A44 44 0 0 1 492 158" fill="none" stroke="#26b8a8" strokeWidth="18" />
      {/* Потоки */}
      <rect x="20" y="200" width="600" height="140" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <rect x="36" y="214" width="190" height="10" rx="5" fill="#e9e9ee" />
      {[0, 1, 2, 3].map((i) => (
        <g key={i}>
          <rect x="36" y={238 + i * 24} width={200 - i * 40} height="14" rx="7" fill="#2aa7f0" opacity={0.85 - i * 0.15} />
          <rect x="250" y={241 + i * 24} width="80" height="8" rx="4" fill="#e9e9ee" />
        </g>
      ))}
      {/* Экспорт */}
      <rect x="500" y="210" width="104" height="24" rx="8" fill="#ff4d14" />
      <text x="552" y="226" fontSize="9" fill="#ffffff" textAnchor="middle">Экспорт ▾ PNG/PDF</text>
    </ShotFrame>
  )
}

// ---------- 11. Добавление статуса (админ) ----------
export function ShotAddStatus() {
  return (
    <ShotFrame title="Добавить статус в workflow">
      <rect x="160" y="60" width="320" height="250" rx="16" fill="#ffffff" stroke="#e8e8ee" />
      <text x="320" y="94" fontSize="14" fontWeight="700" fill="#1b1b1f" textAnchor="middle">
        Добавить статус в workflow
      </text>
      <text x="186" y="128" fontSize="11" fill="#45454d">Название статуса</text>
      <rect x="186" y="136" width="268" height="34" rx="10" fill="#ffffff" stroke="#dcdce3" />
      <text x="200" y="158" fontSize="10" fill="#45454d">Адаптация программы под вуз</text>
      <text x="186" y="192" fontSize="11" fill="#45454d">Вставить после этапа</text>
      <rect x="186" y="200" width="268" height="34" rx="10" fill="#ffffff" stroke="#dcdce3" />
      <text x="200" y="222" fontSize="10" fill="#45454d">Шаг 7 «Передача материалов…»</text>
      <rect x="186" y="244" width="14" height="14" rx="3" fill="#ffffff" stroke="#ff4d14" strokeWidth="2" />
      <text x="208" y="255" fontSize="10" fill="#45454d">Финальный статус</text>
      <rect x="186" y="272" width="130" height="30" rx="10" fill="#ff4d14" />
      <text x="251" y="292" fontSize="11" fontWeight="600" fill="#ffffff" textAnchor="middle">Добавить статус</text>
    </ShotFrame>
  )
}

// ---------- 12. Раздел «Система» (админ) ----------
export function ShotSystem() {
  return (
    <ShotFrame title="Система">
      <rect x="20" y="48" width="180" height="296" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="36" y="74" fontSize="11" fontWeight="700" fill="#1b1b1f">Система</text>
      <rect x="36" y="90" width="148" height="26" rx="8" fill="#fff0e9" />
      <text x="46" y="107" fontSize="9" fill="#c23509">Пользователи и права</text>
      <rect x="36" y="122" width="148" height="26" rx="8" fill="#ffffff" />
      <text x="46" y="139" fontSize="9" fill="#45454d">Интеграции API</text>
      <rect x="36" y="154" width="148" height="26" rx="8" fill="#ffffff" />
      <text x="46" y="171" fontSize="9" fill="#45454d">Журнал аудита</text>
      <rect x="36" y="186" width="148" height="26" rx="8" fill="#ffffff" />
      <text x="46" y="203" fontSize="9" fill="#45454d">Подгрузка JSON</text>
      {/* Таблица пользователей */}
      <rect x="216" y="48" width="404" height="296" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      {[0, 1, 2, 3, 4].map((i) => (
        <g key={i}>
          <circle cx="240" cy={82 + i * 52} r="12" fill="#ffe0d3" />
          <rect x="262" y={72 + i * 52} width="120" height="9" rx="4.5" fill="#e9e9ee" />
          <rect x="262" y={86 + i * 52} width="90" height="8" rx="4" fill="#f2f2f5" />
          <rect x="500" y={72 + i * 52} width="60" height="18" rx="9" fill={i === 0 ? '#f4efff' : i === 1 ? '#e8f5fd' : '#e7f9ef'} />
          <text x="530" y={85 + i * 52} fontSize="9" fill="#45454d" textAnchor="middle">
            {['admin', 'manager', 'user', 'user', 'user'][i]}
          </text>
        </g>
      ))}
    </ShotFrame>
  )
}

// ---------- 13. Профиль ----------
export function ShotProfile() {
  return (
    <ShotFrame title="Профиль">
      <rect x="20" y="48" width="600" height="40" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <circle cx="580" cy="68" r="14" fill="#ff6b3d" />
      <text x="580" y="73" fontSize="11" fontWeight="700" fill="#ffffff" textAnchor="middle">И</text>
      <text x="560" y="72" fontSize="10" fill="#45454d" textAnchor="end">Иванов И. С.</text>
      {/* Меню */}
      <rect x="440" y="100" width="180" height="120" rx="12" fill="#ffffff" stroke="#e8e8ee" />
      <text x="456" y="124" fontSize="10" fontWeight="700" fill="#1b1b1f">Иванов Иван Сергеевич</text>
      <text x="456" y="142" fontSize="9" fill="#70707a">ivanov@edu-rt.ru</text>
      <text x="456" y="158" fontSize="9" fill="#70707a">Роли: user</text>
      <rect x="456" y="170" width="148" height="24" rx="8" fill="#feecea" />
      <text x="468" y="186" fontSize="9" fill="#f9392d">⎋ Выйти</text>
    </ShotFrame>
  )
}

// Реестр иллюстраций: ключ главы -> компонент схемы экрана.
// Используется DocumentationPage.jsx как встроенный fallback,
// когда реальный скриншот (src) не задан или файл не найден.
export const DOC_SCREENSHOTS = {
  login: ShotLogin,
  dashboard: ShotDashboard,
  workflow: ShotWorkflow,
  transition: ShotTransition,
  files: ShotFiles,
  universities: ShotUniversities,
  assignManager: ShotAssignManager,
  directions: ShotDirections,
  reports: ShotReports,
  visualization: ShotVisualization,
  addStatus: ShotAddStatus,
  system: ShotSystem,
  profile: ShotProfile,
}