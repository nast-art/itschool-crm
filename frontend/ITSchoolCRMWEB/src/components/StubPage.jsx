// Заглушка раздела, который команда ещё не реализовала.
// Позволяет навигации по боковому меню работать сразу.
export default function StubPage({ title, description }) {
  return (
    <div className="dashboard">
      <div className="page-head">
        <div>
          <h1>{title}</h1>
          {description && <p className="page-sub">{description}</p>}
        </div>
      </div>
      <section className="panel">
        <p className="empty">
          Раздел «{title}» находится в разработке. Функциональность появится в
          ближайших итерациях.
        </p>
      </section>
    </div>
  )
}