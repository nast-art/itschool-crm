// Общая оболочка экранов "Вход" и "Регистрация" по макету:
// слева — брендовая панель со статистикой и иллюстрацией кампуса,
// справа — карточка с формой.
// Используется на обеих страницах, чтобы они оставались полными аналогами друг друга.

// Декоративная иллюстрация в нижней части брендовой панели
// (в макете — «фон, картинка вузов»). Рисуется inline-SVG в палитре токенов,
// поэтому автоматически подстраивается под цветовую схему.
function CampusArt() {
  return (
    <svg
      className="auth-art"
      viewBox="0 0 440 210"
      fill="none"
      role="img"
      aria-label="Иллюстрация: кампус университета"
    >
      {/* Солнце с ореолом */}
      <circle cx="368" cy="40" r="26" fill="var(--accent-bg-strong)" />
      <circle cx="368" cy="40" r="15" fill="var(--accent-light)" opacity="0.75" />

      {/* Облака */}
      <g fill="#ffffff" opacity="0.85">
        <ellipse cx="90" cy="48" rx="26" ry="10" />
        <ellipse cx="112" cy="42" rx="18" ry="8" />
        <ellipse cx="300" cy="30" rx="20" ry="8" />
      </g>

      {/* Птицы */}
      <path d="M140 60 q6 -6 12 0 q6 -6 12 0" stroke="var(--accent-soft)" strokeWidth="2.5" strokeLinecap="round" />
      <path d="M330 66 q5 -5 10 0 q5 -5 10 0" stroke="var(--accent-soft)" strokeWidth="2.5" strokeLinecap="round" />

      {/* Главный корпус: ступени, тело, фронтон с колоннами */}
      <rect x="158" y="186" width="124" height="7" rx="3" fill="var(--accent-border)" />
      <rect x="166" y="112" width="108" height="76" rx="6" fill="#ffffff" stroke="var(--accent-border)" />
      <path d="M160 114 L220 72 L280 114 Z" fill="var(--accent-soft)" opacity="0.6" />

      {/* Флаг на крыше */}
      <rect x="217" y="42" width="4" height="32" rx="2" fill="var(--accent-soft)" />
      <path d="M221 44 L250 52 L221 60 Z" fill="var(--accent-default)" />

      {/* Колонны */}
      <rect x="178" y="124" width="8" height="64" rx="3" fill="var(--accent-bg-strong)" />
      <rect x="200" y="124" width="8" height="64" rx="3" fill="var(--accent-bg-strong)" />
      <rect x="232" y="124" width="8" height="64" rx="3" fill="var(--accent-bg-strong)" />
      <rect x="254" y="124" width="8" height="64" rx="3" fill="var(--accent-bg-strong)" />

      {/* Вход */}
      <rect x="210" y="152" width="20" height="36" rx="4" fill="var(--accent-default)" opacity="0.85" />

      {/* Левый корпус */}
      <rect x="52" y="132" width="86" height="56" rx="8" fill="var(--accent-bg-strong)" stroke="var(--accent-border)" />
      <circle cx="72" cy="150" r="4" fill="#ffffff" />
      <circle cx="92" cy="150" r="4" fill="#ffffff" />
      <circle cx="112" cy="150" r="4" fill="#ffffff" />
      <circle cx="72" cy="170" r="4" fill="#ffffff" />
      <circle cx="92" cy="170" r="4" fill="#ffffff" />
      <circle cx="112" cy="170" r="4" fill="#ffffff" />

      {/* Правый корпус */}
      <rect x="302" y="126" width="86" height="62" rx="8" fill="var(--accent-bg-strong)" stroke="var(--accent-border)" />
      <circle cx="322" cy="146" r="4" fill="#ffffff" />
      <circle cx="342" cy="146" r="4" fill="#ffffff" />
      <circle cx="362" cy="146" r="4" fill="#ffffff" />
      <circle cx="322" cy="168" r="4" fill="#ffffff" />
      <circle cx="342" cy="168" r="4" fill="#ffffff" />
      <circle cx="362" cy="168" r="4" fill="#ffffff" />

      {/* Деревья */}
      <rect x="30" y="164" width="6" height="24" rx="3" fill="var(--accent-soft)" />
      <circle cx="33" cy="156" r="16" fill="var(--accent-soft)" opacity="0.5" />
      <rect x="404" y="160" width="6" height="28" rx="3" fill="var(--accent-soft)" />
      <circle cx="407" cy="150" r="18" fill="var(--accent-soft)" opacity="0.5" />

      {/* Дорожка */}
      <rect x="20" y="192" width="400" height="8" rx="4" fill="var(--accent-border)" />
    </svg>
  )
}

function BrandPanel() {
  return (
    <aside className="auth-brand">
      <h1 className="auth-brand-title">
        Система контроля обучения <span>ИТ-направлениям</span>
      </h1>

      <div className="auth-stats">
        <div className="auth-stat">
          <strong>120+</strong>
          <span>ВУЗОВ</span>
        </div>
        <div className="auth-stat">
          <strong>38</strong>
          <span>ИТ-ПРОГРАММ</span>
        </div>
        <div className="auth-stat">
          <strong>14</strong>
          <span>ШАГОВ WORKFLOW</span>
        </div>
      </div>

      <div className="auth-brand-art" aria-hidden="true">
        <CampusArt />
        <p>Федеральные проекты · Вузы партнёры · LMS</p>
      </div>
    </aside>
  )
}

export default function AuthShell({ children }) {
  return (
    <div className="auth-screen">
      <div className="auth-shell">
        <BrandPanel />
        <main className="auth-form-side">{children}</main>
      </div>
    </div>
  )
}