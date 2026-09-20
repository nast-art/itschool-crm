import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { getStatistics, downloadReport } from '../api/reports'
import {
  consumePendingUniversityFilter,
  onUniversityFilter,
} from '../state/dashboardFilter.js'
import {
  IconChevronDown,
  IconDownload,
  IconReport,
  IconArrowRight,
  IconBuildingActive,
  IconSwap,
  IconLicense,
  IconSignature,
} from '../components/icons.jsx'
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  PieChart,
  Pie,
  Cell,
} from 'recharts'

// Периоды обзора — календарные: неделя с понедельника, месяц с 1-го числа,
// квартал с 1-го дня текущего квартала, год с 1 января.
const PERIODS = [
  { id: 'week', label: 'Неделя' },
  { id: 'month', label: 'Месяц' },
  { id: 'quarter', label: 'Квартал' },
  { id: 'year', label: 'Год' },
]

// Начало календарного периода по его id
function periodStart(id) {
  const now = new Date()

  if (id === 'week') {
    // с понедельника текущей недели
    const mondayOffset = (now.getDay() + 6) % 7
    return new Date(
      now.getFullYear(),
      now.getMonth(),
      now.getDate() - mondayOffset,
    )
  }

  if (id === 'month') {
    return new Date(now.getFullYear(), now.getMonth(), 1)
  }

  if (id === 'quarter') {
    const quarterFirstMonth = Math.floor(now.getMonth() / 3) * 3
    return new Date(now.getFullYear(), quarterFirstMonth, 1)
  }

  if (id === 'year') {
    return new Date(now.getFullYear(), 0, 1)
  }

  return now
}

const MONTHS = ['Янв', 'Фев', 'Мар', 'Апр', 'Май', 'Июн', 'Июл', 'Авг', 'Сен', 'Окт', 'Ноя', 'Дек']

// Палитра диаграммы — статусные цвета ДС Ростелеком
const PIE_COLORS = ['#8b31ff', '#5b7cfa', '#2aa7f0', '#26b8a8', '#d73bd0', '#f0447e', '#ff4d14']

// Восстановление сохранённых фильтров из localStorage.
// Фильтры переживают перезагрузку страницы (кэш действий пользователя).
function readStoredIds(key) {
  try {
    const raw = localStorage.getItem(key)
    const parsed = raw ? JSON.parse(raw) : []
    return Array.isArray(parsed) ? parsed.filter((x) => Number.isInteger(x)) : []
  } catch {
    return []
  }
}

function Kpi({ icon, label, value, hint }) {
  return (
    <div className="kpi">
      <span className="kpi-icon">{icon}</span>
      <div className="kpi-body">
        <span className="kpi-label">{label}</span>
        <strong className="kpi-value">{value}</strong>
        {hint && <span className="kpi-hint">{hint}</span>}
      </div>
    </div>
  )
}

// Список -> карта «имя в нижнем регистре -> количество»
function toCountMap(list) {
  const map = {}
  ;(list ?? []).forEach((x) => {
    map[x.name.toLowerCase()] = x.count
  })
  return map
}

export default function DashboardPage() {
  const [universities, setUniversities] = useState([])
  const [directions, setDirections] = useState([])
  const [programs, setPrograms] = useState([])
  const [products, setProducts] = useState([])
  const [interactions, setInteractions] = useState([])
  const [statistics, setStatistics] = useState(null)
  // Списки для меню фильтров: ПО ВСЕМУ периоду без фильтров,
  // чтобы пункты не исчезали из списка при выборе
  const [menuStatistics, setMenuStatistics] = useState(null)
  // Счётчики для пунктов меню: сколько попадёт в выборку при выборе пункта
  // (с учётом остальных фильтров, но без фильтра своей секции — иначе
  // выбор одного вуза обнулял бы счётчики у остальных вузов)
  const [uniFacetCounts, setUniFacetCounts] = useState({})
  const [dirFacetCounts, setDirFacetCounts] = useState({})
  const [prodFacetCounts, setProdFacetCounts] = useState({})
  const [period, setPeriod] = useState(() => localStorage.getItem('dash_period') ?? 'month')
  const [periodOpen, setPeriodOpen] = useState(false)
  const [reportOpen, setReportOpen] = useState(false)
  // Фильтры (мультивыбор): вузы, направления, продукты (по ТЗ).
  // Восстанавливаются из localStorage после перезагрузки страницы.
  const [selectedUniversityIds, setSelectedUniversityIds] = useState(() =>
    readStoredIds('dash_filter_universities'),
  )
  const [selectedDirectionIds, setSelectedDirectionIds] = useState(() =>
    readStoredIds('dash_filter_directions'),
  )
  const [selectedProductIds, setSelectedProductIds] = useState(() =>
    readStoredIds('dash_filter_products'),
  )
  const [exporting, setExporting] = useState(false)
  const [error, setError] = useState('')

  // Загрузка справочников и взаимодействий — один раз
  useEffect(() => {
    Promise.all([
      api.get('/Universities'),
      api.get('/Interactions'),
      api.get('/Directions'),
      api.get('/Programs'),
      api.get('/Products'),
    ])
      .then(([unis, list, dirs, progs, prods]) => {
        setUniversities(unis ?? [])
        setInteractions(list ?? [])
        setDirections(dirs ?? [])
        setPrograms(progs ?? [])
        setProducts(prods ?? [])
      })
      .catch((err) => setError(err.message))
  }, [])

  // Фильтр из поиска в верхней панели
  useEffect(() => {
    const pending = consumePendingUniversityFilter()
    if (pending) setSelectedUniversityIds([pending])
    return onUniversityFilter((id) => setSelectedUniversityIds([id]))
  }, [])

  const currentPeriod = PERIODS.find((p) => p.id === period) ?? PERIODS[1]

  // Сохранение фильтров при любом их изменении (переживают перезагрузку)
  useEffect(() => {
    localStorage.setItem('dash_period', period)
  }, [period])

  useEffect(() => {
    localStorage.setItem('dash_filter_universities', JSON.stringify(selectedUniversityIds))
  }, [selectedUniversityIds])

  useEffect(() => {
    localStorage.setItem('dash_filter_directions', JSON.stringify(selectedDirectionIds))
  }, [selectedDirectionIds])

  useEffect(() => {
    localStorage.setItem('dash_filter_products', JSON.stringify(selectedProductIds))
  }, [selectedProductIds])

  // Фильтр для API статистики и экспорта: период + вузы + направления + продукты
  const filter = useMemo(
    () => ({
      dateFrom: periodStart(currentPeriod.id).toISOString(),
      universityIds: selectedUniversityIds,
      directionIds: selectedDirectionIds,
      productIds: selectedProductIds,
    }),
    [currentPeriod, selectedUniversityIds, selectedDirectionIds, selectedProductIds],
  )

  // Основная статистика (с фильтрами) — для KPI и диаграмм
  useEffect(() => {
    getStatistics(filter)
      .then(setStatistics)
      .catch((err) => setError(err.message))
  }, [filter])

  // Списки для меню фильтров: пересчитываются только при смене периода,
  // без учёта выбранных фильтров (чтобы пункты не исчезали из списка)
  useEffect(() => {
    getStatistics({ dateFrom: periodStart(currentPeriod.id).toISOString() })
      .then(setMenuStatistics)
      .catch(() => {})
  }, [currentPeriod])

  // Счётчики пунктов меню: каждая секция считается БЕЗ фильтра своей секции,
  // но С остальными фильтрами — цифра = сколько попадёт в выборку,
  // если выбрать этот пункт
  useEffect(() => {
    const base = { dateFrom: periodStart(currentPeriod.id).toISOString() }

    getStatistics({
      ...base,
      directionIds: selectedDirectionIds,
      productIds: selectedProductIds,
    })
      .then((s) => setUniFacetCounts(toCountMap(s.byUniversity)))
      .catch(() => {})

    getStatistics({
      ...base,
      universityIds: selectedUniversityIds,
      productIds: selectedProductIds,
    })
      .then((s) => setDirFacetCounts(toCountMap(s.byDirection)))
      .catch(() => {})

    getStatistics({
      ...base,
      universityIds: selectedUniversityIds,
      directionIds: selectedDirectionIds,
    })
      .then((s) => setProdFacetCounts(toCountMap(s.byProduct)))
      .catch(() => {})
  }, [currentPeriod, selectedUniversityIds, selectedDirectionIds, selectedProductIds])

  // Соответствие программы и направления (для клиентской фильтрации)
  const programDirectionMap = useMemo(() => {
    const map = {}
    programs.forEach((p) => {
      map[p.id] = p.directionId
    })
    return map
  }, [programs])

  // Клиентская фильтрация взаимодействий (график по месяцам, лидеры, лицензии)
  const filtered = useMemo(() => {
    const from = periodStart(currentPeriod.id).getTime()
    return interactions.filter((i) => {
      if (selectedUniversityIds.length && !selectedUniversityIds.includes(i.universityId)) {
        return false
      }
      if (
        selectedDirectionIds.length &&
        !selectedDirectionIds.includes(programDirectionMap[i.programId])
      ) {
        return false
      }
      if (selectedProductIds.length && !selectedProductIds.includes(i.productId)) {
        return false
      }
      const time = i.createdAt ? new Date(i.createdAt).getTime() : 0
      return time >= from
    })
  }, [interactions, currentPeriod, selectedUniversityIds, selectedDirectionIds, selectedProductIds, programDirectionMap])

  // Заявки по месяцам
  const monthly = useMemo(() => {
    const map = {}
    filtered.forEach((i) => {
      if (!i.createdAt) return
      const d = new Date(i.createdAt)
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
      map[key] = (map[key] ?? 0) + 1
    })
    return Object.entries(map)
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([key, count]) => ({
        name: MONTHS[Number(key.slice(5)) - 1] ?? key,
        count,
      }))
  }, [filtered])

  const byStatus = statistics?.byStatus ?? []
  const byDirection = (statistics?.byDirection ?? []).slice(0, 6)

  // KPI-показатели — все из ОДНОГО отфильтрованного набора (период + фильтры)
  const activeUniversities = statistics?.byUniversity?.length ?? 0
  const inWorkNow = filtered.length
  const licensesCount = filtered.filter((i) => i.licenseId).length
  const signingCount = byStatus
    .filter((s) => s.name.toLowerCase().includes('подпис'))
    .reduce((sum, s) => sum + s.count, 0)

  function toggleId(list, setList, id) {
    setList(list.includes(id) ? list.filter((x) => x !== id) : [...list, id])
  }

  async function handleExport() {
    setExporting(true)
    setError('')
    try {
      await downloadReport('xlsx', filter)
    } catch (err) {
      setError(err.message)
    } finally {
      setExporting(false)
    }
  }

  // Подписи для чипов выбранных фильтров
  const universityChip = (id) => {
    const u = universities.find((x) => x.id === id)
    return u ? u.shortName || u.name : `Вуз #${id}`
  }
  const directionChip = (id) => directions.find((x) => x.id === id)?.name ?? `Направление #${id}`
  const productChip = (id) => products.find((x) => x.id === id)?.name ?? `Продукт #${id}`

  const hasAnyChip =
    selectedUniversityIds.length > 0 ||
    selectedDirectionIds.length > 0 ||
    selectedProductIds.length > 0

  return (
    <div className="dashboard">
      <div className="page-head">
        <div>
          <h1>Главная</h1>
          <p className="page-sub">Обзор взаимодействий за период</p>
        </div>

        {/* Кнопки действий: всегда на одном месте, чипы их не трогают */}
        <div className="page-actions">
          {/* Выбор периода */}
          <div className="menu-wrap">
            <button
              className="btn-ghost"
              onClick={() => {
                setPeriodOpen((v) => !v)
                setReportOpen(false)
              }}
            >
              {currentPeriod.label}
              <IconChevronDown size={16} />
            </button>
            {periodOpen && (
              <div className="menu">
                {PERIODS.map((p) => (
                  <button
                    key={p.id}
                    className={p.id === period ? 'menu-item active' : 'menu-item'}
                    onClick={() => {
                      setPeriod(p.id)
                      setPeriodOpen(false)
                    }}
                  >
                    {p.label}
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Экспорт */}
          <button className="btn-ghost" onClick={handleExport} disabled={exporting}>
            <IconDownload size={16} />
            {exporting ? 'Экспорт…' : 'Экспорт'}
          </button>

          {/* Отчёт: фильтры по вузам, направлениям, продуктам (мультивыбор).
              Статусы — справочно: фильтр по статусам в ТЗ не требуется. */}
          <div className="menu-wrap">
            <button
              className="btn-accent-soft"
              onClick={() => {
                setReportOpen((v) => !v)
                setPeriodOpen(false)
              }}
            >
              <IconReport size={16} />
              Отчёт
              <IconChevronDown size={16} />
            </button>
            {reportOpen && (
              <div className="menu menu--wide">
                <div className="menu-hint">
                  Цифры показывают, сколько взаимодействий попадёт в выборку,
                  если выбрать пункт (с учётом остальных фильтров)
                </div>

                <div className="menu-group">Вузы</div>
                {(menuStatistics?.byUniversity ?? []).slice(0, 8).map((u) => {
                  const id = universities.find(
                    (x) => x.name.toLowerCase() === u.name.toLowerCase(),
                  )?.id
                  const active = id != null && selectedUniversityIds.includes(id)
                  const count = uniFacetCounts[u.name.toLowerCase()] ?? 0
                  return (
                    <button
                      key={u.name}
                      className={active ? 'menu-item active' : 'menu-item'}
                      disabled={id == null}
                      onClick={() => id != null && toggleId(selectedUniversityIds, setSelectedUniversityIds, id)}
                    >
                      <span className="menu-text">{u.name}</span>
                      {active ? (
                        <span className="menu-check">✓</span>
                      ) : (
                        <span className={count === 0 ? 'menu-count is-zero' : 'menu-count'}>{count}</span>
                      )}
                    </button>
                  )
                })}
                {selectedUniversityIds.length > 0 && (
                  <button className="menu-item menu-clear" onClick={() => setSelectedUniversityIds([])}>
                    Сбросить выбор вузов
                  </button>
                )}

                <div className="menu-group">Направления</div>
                {(menuStatistics?.byDirection ?? []).slice(0, 6).map((d) => {
                  const id = directions.find(
                    (x) => x.name.toLowerCase() === d.name.toLowerCase(),
                  )?.id
                  const active = id != null && selectedDirectionIds.includes(id)
                  const count = dirFacetCounts[d.name.toLowerCase()] ?? 0
                  return (
                    <button
                      key={d.name}
                      className={active ? 'menu-item active' : 'menu-item'}
                      disabled={id == null}
                      onClick={() => id != null && toggleId(selectedDirectionIds, setSelectedDirectionIds, id)}
                    >
                      <span className="menu-text">{d.name}</span>
                      {active ? (
                        <span className="menu-check">✓</span>
                      ) : (
                        <span className={count === 0 ? 'menu-count is-zero' : 'menu-count'}>{count}</span>
                      )}
                    </button>
                  )
                })}

                <div className="menu-group">Продукты</div>
                {(menuStatistics?.byProduct ?? []).slice(0, 8).map((p) => {
                  const id = products.find(
                    (x) => x.name.toLowerCase() === p.name.toLowerCase(),
                  )?.id
                  const active = id != null && selectedProductIds.includes(id)
                  const count = prodFacetCounts[p.name.toLowerCase()] ?? 0
                  return (
                    <button
                      key={p.name}
                      className={active ? 'menu-item active' : 'menu-item'}
                      disabled={id == null}
                      onClick={() => id != null && toggleId(selectedProductIds, setSelectedProductIds, id)}
                    >
                      <span className="menu-text">{p.name}</span>
                      {active ? (
                        <span className="menu-check">✓</span>
                      ) : (
                        <span className={count === 0 ? 'menu-count is-zero' : 'menu-count'}>{count}</span>
                      )}
                    </button>
                  )
                })}

                <div className="menu-group">Статусы (справочно)</div>
                {(menuStatistics?.byStatus ?? []).slice(0, 6).map((s) => (
                  <div key={s.name} className="menu-item menu-item--static">
                    <span className="menu-text">{s.name}</span>
                    <span className="menu-count">{s.count}</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Отдельная строка чипов: переносится сама по себе и не двигает кнопки */}
      {hasAnyChip && (
        <div className="chips-row">
          {selectedUniversityIds.map((id) => (
            <button
              key={`uni-${id}`}
              className="chip"
              onClick={() => toggleId(selectedUniversityIds, setSelectedUniversityIds, id)}
              title="Убрать фильтр"
            >
              <span className="chip-text">{universityChip(id)}</span> ✕
            </button>
          ))}
          {selectedDirectionIds.map((id) => (
            <button
              key={`dir-${id}`}
              className="chip"
              onClick={() => toggleId(selectedDirectionIds, setSelectedDirectionIds, id)}
              title="Убрать фильтр"
            >
              <span className="chip-text">{directionChip(id)}</span> ✕
            </button>
          ))}
          {selectedProductIds.map((id) => (
            <button
              key={`prod-${id}`}
              className="chip"
              onClick={() => toggleId(selectedProductIds, setSelectedProductIds, id)}
              title="Убрать фильтр"
            >
              <span className="chip-text">{productChip(id)}</span> ✕
            </button>
          ))}
        </div>
      )}

      {error && <div className="form-error">{error}</div>}

      {/* KPI-показатели */}
      <section className="kpi-grid">
        <Kpi icon={<IconBuildingActive />} label="Активные вузы" value={activeUniversities} hint="за период" />
        <Kpi icon={<IconSwap />} label="В работе сейчас" value={inWorkNow} hint="взаимодействий" />
        <Kpi icon={<IconLicense />} label="Продлить лицензии" value={licensesCount} hint="требуют внимания" />
        <Kpi icon={<IconSignature />} label="Время до подписания" value={signingCount} hint="на этапе подписания" />
      </section>

      {/* Графики */}
      <section className="dash-grid">
        <div className="panel">
          <h2>Статистика заявок за период</h2>
          <p className="panel-sub">график по месяцам</p>
          <div className="chart">
            {monthly.length === 0 ? (
              <p className="empty">Нет данных за выбранный период</p>
            ) : (
              <ResponsiveContainer width="100%" height={240}>
                <BarChart data={monthly} margin={{ top: 8, right: 8, left: -18, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border-soft)" vertical={false} />
                  <XAxis dataKey="name" tick={{ fontSize: 12, fill: 'var(--fg-muted)' }} axisLine={false} tickLine={false} />
                  <YAxis tick={{ fontSize: 12, fill: 'var(--fg-muted)' }} axisLine={false} tickLine={false} allowDecimals={false} />
                  <Tooltip cursor={{ fill: 'var(--accent-bg)' }} />
                  <Bar dataKey="count" name="Заявки" fill="var(--accent-default)" radius={[6, 6, 0, 0]} maxBarSize={42} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        <div className="panel">
          <h2>Популярность ИТ-направлений</h2>
          <p className="panel-sub">диаграмма</p>
          <div className="chart">
            {byDirection.length === 0 ? (
              <p className="empty">Нет данных за выбранный период</p>
            ) : (
              <>
                <ResponsiveContainer width="100%" height={200}>
                  <PieChart>
                    <Pie
                      data={byDirection}
                      dataKey="count"
                      nameKey="name"
                      innerRadius={52}
                      outerRadius={86}
                      paddingAngle={3}
                    >
                      {byDirection.map((entry, index) => (
                        <Cell key={entry.name} fill={PIE_COLORS[index % PIE_COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
                <ul className="pie-legend">
                  {byDirection.map((entry, index) => (
                    <li key={entry.name}>
                      <span className="pie-dot" style={{ background: PIE_COLORS[index % PIE_COLORS.length] }} />
                      {entry.name}
                      <strong>{entry.count}</strong>
                    </li>
                  ))}
                </ul>
              </>
            )}
          </div>
        </div>
      </section>

      {/* Лидеры по программам */}
      <section className="panel">
        <div className="panel-head">
          <div>
            <h2>Лидеры по программам</h2>
            <p className="panel-sub">5 вузов: направление, рейтинг, статус, ответственный</p>
          </div>
          <Link to="/universities" className="btn-ghost">
            Рейтинг всех вузов
            <IconArrowRight size={16} />
          </Link>
        </div>

        {filtered.length === 0 ? (
          <p className="empty">Пока нет взаимодействий за выбранный период.</p>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Рейтинг</th>
                  <th>Вуз</th>
                  <th>Направление</th>
                  <th>Статус</th>
                  <th>Ответственный</th>
                </tr>
              </thead>
              <tbody>
                {filtered.slice(0, 5).map((item, index) => (
                  <tr key={item.id}>
                    <td>#{index + 1}</td>
                    <td>{item.universityName ?? '—'}</td>
                    <td>{item.programName ?? '—'}</td>
                    <td>
                      {item.currentStatusName && (
                        <span className="status-badge">{item.currentStatusName}</span>
                      )}
                    </td>
                    <td>{item.managerName ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}