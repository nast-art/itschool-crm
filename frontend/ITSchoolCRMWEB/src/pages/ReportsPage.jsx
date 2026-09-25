// Страница «Отчёты» — формирование отчётов за выбранный период.
// Состав по макету:
// 1. Шапка: «Отчёты» + подзаголовок
//    «Формирование отчётов за выбранный период в форматах xls, xlsx, pdf».
// 2. Панель фильтров: Период (диапазон дат «с… по…»), Вуз,
//    ИТ-направление, ИТ-продукт, Ответственный + кнопки «Применить» /
//    «Сбросить». Сетка адаптивная (auto-fill): поля переносятся на
//    следующую строку и не сжимаются.
// 3. Блок «Выбор колонок отчёта» — чекбоксы с галочкой при выборе:
//    Наименование вуза · ИТ-направление · ИТ-продукт · Статус ·
//    Ответственный · Вендор · № Договора. Доступны ВСЕМ ролям —
//    требование 4 ТЗ деления колонок по ролям не предусматривает,
//    а пользователь по ТЗ вправе формировать отчёты.
// 4. Предпросмотр таблицы с выбранными колонками + пагинация.
//    Пользователь сам выбирает, сколько строк показывать на странице
//    (10 / 20 / 50 / 100). Пагинация ВЛИЯЕТ ТОЛЬКО НА ПРЕДПРОСМОТР —
//    экспорт формируется ВСЕГДА по всей выборке, подходящей под
//    фильтр (требование 2 ТЗ: «отчёты по ВСЕМ существующим
//    взаимодействиям за выбранный период»).
// 5. Экспорт: xls, xlsx, pdf — для всех ролей (требования 4 и 8 ТЗ);
//    результирующий json — администратору (п. 11 ТЗ: «расширенные
//    права»; п. 4 «Требований к решению»: решение должно уметь
//    формировать json).
//
// РОЛЕВАЯ МОДЕЛЬ (строго по п. 11 ТЗ):
// user    — работа с данными + формирование отчётов (xls/xlsx/pdf);
// manager — то же + изменение ответственных за вузы (страница «Вузы»);
// admin   — то же + управление правами и настройки (+ json-экспорт здесь).
// Разграничение по ДАННЫМ (пользователь видит только свои вузы) выполняет
// бэкенд через university_managers — фронт дополнительно не ограничивает.
//
// Контракт с бэкендом (ITSchoolCRM.API):
// GET  /Interactions        — взаимодействия (уже отфильтрованы по доступу)
// GET  /Universities        — справочник вузов (опции фильтра)
// GET  /Directions          — справочник направлений
// GET  /Programs            — программы (нужны для фильтра по направлению)
// GET  /Products            — продукты (колонка «Вендор»)
// GET  /Contracts           — договоры (колонка «№ Договора»)
// GET  /Users               — пользователи (фильтр «Ответственный»)
// POST /Reports/{format}    — формирование отчёта (xls|xlsx|pdf|json),
//                             в теле — ReportFilterDto:
//                             dateFrom, dateTo, universityIds, directionIds,
//                             productIds, managerIds, columns (массив
//                             ключей колонок из ALL_COLUMNS)
import { useEffect, useMemo, useState } from 'react'
import { api } from '../api/client'
import { downloadReport } from '../api/reports.js'
import { getContracts } from '../api/universities.js'
import { useAuth } from '../App.jsx'
import { IconDownload, IconReport } from '../components/icons.jsx'

// ---------- Доступные колонки отчёта ----------

// Обязательный перечень из требования 4 ТЗ: «наименование вуза,
// ИТ-направление, ИТ-продукт, статус работы с вузом, ответственный»
// + колонки по макету («Вендор», «№ Договора»). Доступны ВСЕМ ролям.
// Ключи — это значения, которые уходят в поле columns фильтра отчёта.
const ALL_COLUMNS = [
  { key: 'university', label: 'Наименование вуза' },
  { key: 'direction', label: 'ИТ-направление' },
  { key: 'product', label: 'ИТ-продукт' },
  { key: 'status', label: 'Статус' },
  { key: 'manager', label: 'Ответственный' },
  { key: 'vendor', label: 'Вендор' },
  { key: 'contractNumber', label: '№ Договора' },
]

// Форматы экспорта (требования 4 и 8 ТЗ: xls, xlsx, pdf) — для всех ролей
const EXPORT_FORMATS = [
  { id: 'xls', label: 'XLS' },
  { id: 'xlsx', label: 'XLSX' },
  { id: 'pdf', label: 'PDF' },
]

// Результирующий json (п. 4 «Требований к решению») — расширенная
// привилегия администратора (п. 11 ТЗ: «расширенные права»)
const JSON_FORMAT = { id: 'json', label: 'JSON' }

// Варианты размера страницы предпросмотра (пользователь выбирает сам).
// Значение сохраняется в localStorage — кэш действий пользователя
// (нефункциональное требование 13).
const PAGE_SIZE_OPTIONS = [10, 20, 50, 100]
const PAGE_SIZE_STORAGE_KEY = 'reports_page_size'

// ---------- Ключевые слова для тонов бейджа статуса ----------
// Матчим по подстрокам, т.к. названия этапов могут отличаться
// («Завершено», «Договор подписан» и т.п.)
const FINAL_STATUS_KEYWORDS = ['заверш', 'подписан', 'закрыт', 'успеш']
const NEW_STATUS_KEYWORDS = ['поиск', 'новый', 'новая', 'новое']

// Тон бейджа: финальные статусы — зелёные, стартовые — синие,
// остальные («в работе») — фиолетовые
function statusToneClass(status) {
  const s = (status ?? '').toLowerCase()
  if (FINAL_STATUS_KEYWORDS.some((k) => s.includes(k))) return 'is-final'
  if (NEW_STATUS_KEYWORDS.some((k) => s.includes(k))) return 'is-new'
  return 'is-progress'
}

// ---------- Форматирование ----------

// Время приходит в UTC без суффикса Z — дописываем Z,
// чтобы браузер сделал сдвиг пояса
function asDate(iso) {
  if (!iso) return null
  const s =
    typeof iso === 'string' && !iso.endsWith('Z') && !/[+-]\d{2}:?\d{2}$/.test(iso)
      ? iso + 'Z'
      : iso
  const d = new Date(s)
  return Number.isNaN(d.getTime()) ? null : d
}

function formatDate(iso) {
  const d = asDate(iso)
  return d ? d.toLocaleDateString('ru-RU') : '—'
}

function userLabel(u) {
  if (!u) return '—'
  const name = [u.lastName, u.firstName, u.middleName].filter(Boolean).join(' ').trim()
  return name || u.fullName || u.email || `Пользователь #${u.id}`
}

// Поле фильтра: подпись сверху, элемент снизу (по макету)
function FilterField({ label, children }) {
  return (
    <label className="field filter-field">
      <span className="field-label">{label}</span>
      {children}
    </label>
  )
}

// ---------- Главный компонент ----------

export default function ReportsPage() {
  const { user } = useAuth()

  // Ролевая модель (п. 11 ТЗ). На этой странице единственное различие —
  // json-экспорт для администратора («расширенные права»).
  // Колонки, фильтры и форматы xls/xlsx/pdf доступны ВСЕМ ролям:
  // «Пользователь сервиса должен иметь возможность … формировать отчёты
  // … в форматах xls, xlsx, pdf».
  const roles = user?.roles ?? []
  const isAdmin = roles.includes('admin')

  // ---------- Справочники ----------

  const [interactions, setInteractions] = useState([])
  const [universities, setUniversities] = useState([])
  const [directions, setDirections] = useState([])
  const [programs, setPrograms] = useState([])
  const [products, setProducts] = useState([])
  const [users, setUsers] = useState([])
  const [contracts, setContracts] = useState([])
  const [loading, setLoading] = useState(true)

  // ---------- Черновики фильтров и применённые фильтры ----------

  const [draftFrom, setDraftFrom] = useState('')
  const [draftTo, setDraftTo] = useState('')
  const [draftUniversity, setDraftUniversity] = useState('')
  const [draftDirection, setDraftDirection] = useState('')
  const [draftProduct, setDraftProduct] = useState('')
  const [draftManager, setDraftManager] = useState('')
  const [filters, setFilters] = useState({
    from: '',
    to: '',
    university: '',
    direction: '',
    product: '',
    manager: '',
  })

  // ---------- Выбор колонок ----------

  // По умолчанию отмечены все колонки макета
  const [selectedColumns, setSelectedColumns] = useState(() =>
    ALL_COLUMNS.map((c) => c.key),
  )

  // ---------- Пагинация предпросмотра ----------

  // Размер страницы пользователь выбирает сам; выбор сохраняется
  // в localStorage (переживает перезагрузку — кэш действий пользователя,
  // нефункциональное требование 13)
  const [pageSize, setPageSize] = useState(() => {
    const stored = Number(localStorage.getItem(PAGE_SIZE_STORAGE_KEY))
    return PAGE_SIZE_OPTIONS.includes(stored) ? stored : 20
  })
  const [page, setPage] = useState(1)
  const [pageInput, setPageInput] = useState('1')

  // ---------- Экспорт ----------

  const [exporting, setExporting] = useState('')
  const [pageError, setPageError] = useState('')
  const [exportNotice, setExportNotice] = useState('')

  // ---------- Загрузка данных (один раз) ----------

  useEffect(() => {
    setLoading(true)
    // Договоры — необязательный справочник: если эндпоинта ещё нет,
    // колонка «№ Договора» просто покажет «—»
    Promise.all([
      api.get('/Interactions'),
      api.get('/Universities'),
      api.get('/Directions'),
      api.get('/Programs'),
      api.get('/Products'),
      api.get('/Users'),
      getContracts().catch(() => []),
    ])
      .then(([list, unis, dirs, progs, prods, usrs, contr]) => {
        setInteractions(list ?? [])
        setUniversities(unis ?? [])
        setDirections(dirs ?? [])
        setPrograms(progs ?? [])
        setProducts(prods ?? [])
        setUsers((usrs ?? []).filter((u) => u.isActive !== false))
        setContracts(contr ?? [])
      })
      .catch((err) => setPageError(err.message))
      .finally(() => setLoading(false))
  }, [])

  // ---------- Сборка строк отчёта ----------

  const programById = useMemo(
    () => Object.fromEntries(programs.map((p) => [p.id, p])),
    [programs],
  )
  const directionById = useMemo(
    () => Object.fromEntries(directions.map((d) => [d.id, d])),
    [directions],
  )
  const productById = useMemo(
    () => Object.fromEntries(products.map((p) => [p.id, p])),
    [products],
  )
  const contractById = useMemo(
    () => Object.fromEntries(contracts.map((c) => [c.id, c])),
    [contracts],
  )

  // Полная выборка строк: каждая строка содержит значения ВСЕХ колонок,
  // дальше она проецируется на выбранные
  const allRows = useMemo(
    () =>
      interactions.map((i) => {
        const program = programById[i.programId] ?? null
        const direction =
          program?.directionId != null
            ? directionById[program.directionId] ?? null
            : null
        const product = i.productId != null ? productById[i.productId] ?? null : null
        const contract =
          i.contractId != null ? contractById[i.contractId] ?? null : null
        return {
          id: i.id,
          university: i.universityName ?? '—',
          direction: direction?.name ?? '—',
          product: i.productName ?? product?.name ?? '—',
          status: i.currentStatusName ?? '—',
          manager: i.managerName ?? '—',
          vendor: product?.vendor ?? '—',
          contractNumber: contract?.contractNumber ?? '—',
          createdAt: i.createdAt ?? null,
          universityId: i.universityId ?? null,
          programId: i.programId ?? null,
          productId: i.productId ?? null,
          managerId: i.managerId ?? null,
        }
      }),
    [interactions, programById, directionById, productById, contractById],
  )

  // ---------- Фильтрация (по кнопке «Применить») ----------

  // ВАЖНО: filtered — это ПОЛНАЯ выборка под фильтр. Именно она уходит
  // в экспорт. Пагинация ниже режет только отображение предпросмотра.
  const filtered = useMemo(() => {
    return allRows.filter((row) => {
      // Период: взаимодействия, созданные внутри диапазона дат
      const createdAt = row.createdAt ? new Date(row.createdAt).getTime() : 0
      if (filters.from) {
        const from = new Date(filters.from + 'T00:00:00').getTime()
        if (createdAt < from) return false
      }
      if (filters.to) {
        // Конец дня включительно
        const to = new Date(filters.to + 'T23:59:59').getTime()
        if (createdAt > to) return false
      }
      if (filters.university && row.universityId !== Number(filters.university)) {
        return false
      }
      if (filters.direction) {
        const program = programById[row.programId]
        if (!program || program.directionId !== Number(filters.direction)) {
          return false
        }
      }
      if (filters.product && row.productId !== Number(filters.product)) {
        return false
      }
      if (filters.manager && row.managerId !== Number(filters.manager)) {
        return false
      }
      return true
    })
  }, [allRows, filters, programById])

  function applyFilters() {
    setFilters({
      from: draftFrom,
      to: draftTo,
      university: draftUniversity,
      direction: draftDirection,
      product: draftProduct,
      manager: draftManager,
    })
    setPage(1)
    setExportNotice('')
  }

  function resetFilters() {
    setDraftFrom('')
    setDraftTo('')
    setDraftUniversity('')
    setDraftDirection('')
    setDraftProduct('')
    setDraftManager('')
    setFilters({
      from: '',
      to: '',
      university: '',
      direction: '',
      product: '',
      manager: '',
    })
    setPage(1)
    setExportNotice('')
  }

  // ---------- Колонки (без деления по ролям) ----------

  const visibleColumns = useMemo(
    () => ALL_COLUMNS.filter((c) => selectedColumns.includes(c.key)),
    [selectedColumns],
  )

  function toggleColumn(key) {
    setSelectedColumns((list) =>
      list.includes(key) ? list.filter((k) => k !== key) : [...list, key],
    )
    setExportNotice('')
  }

  // ---------- Пагинация предпросмотра ----------

  // При смене фильтров или размера страницы возвращаемся на 1-ю страницу
  useEffect(() => {
    setPage(1)
  }, [filters, pageSize])

  useEffect(() => {
    localStorage.setItem(PAGE_SIZE_STORAGE_KEY, String(pageSize))
  }, [pageSize])

  const pageCount = Math.max(1, Math.ceil(filtered.length / pageSize))
  // Защита: если текущая страница стала больше числа страниц
  // (например, после смены фильтра) — показываем последнюю допустимую
  const safePage = Math.min(page, pageCount)

  const paged = useMemo(
    () => filtered.slice((safePage - 1) * pageSize, safePage * pageSize),
    [filtered, safePage, pageSize],
  )

  const shownFrom = filtered.length === 0 ? 0 : (safePage - 1) * pageSize + 1
  const shownTo = Math.min(safePage * pageSize, filtered.length)

  useEffect(() => {
    setPageInput(String(safePage))
  }, [safePage])

  function goToPage(n) {
    setPage(Math.min(Math.max(1, n), pageCount))
  }

  function handlePageInputChange(e) {
    const value = e.target.value
    if (/^\d{0,4}$/.test(value)) {
      setPageInput(value)
    }
  }

  function commitPageInput() {
    const n = parseInt(pageInput, 10)
    if (Number.isNaN(n) || n < 1) {
      setPageInput(String(safePage))
      return
    }
    goToPage(n)
    setPageInput(String(Math.min(n, pageCount)))
  }

  // ---------- Формирование отчёта (экспорт) ----------

  // Тело запроса — РОВНО ТО, что ждёт бэкендный ReportFilterDto:
  //   dateFrom, dateTo    — ISO-строки или null;
  //   universityIds, directionIds, productIds, managerIds — массивы id
  //                         (пустой массив = фильтр не применяется);
  //   columns             — массив ключей выбранных колонок из
  //                         ALL_COLUMNS (требование 4 ТЗ).
  // Пагинация НЕ передаётся — бэкенд формирует файл по ВСЕЙ выборке
  // под фильтр (требование 2 ТЗ).
  function buildReportPayload() {
    return {
      dateFrom: filters.from ? new Date(filters.from + 'T00:00:00').toISOString() : null,
      dateTo: filters.to ? new Date(filters.to + 'T23:59:59').toISOString() : null,
      universityIds: filters.university ? [Number(filters.university)] : [],
      directionIds: filters.direction ? [Number(filters.direction)] : [],
      productIds: filters.product ? [Number(filters.product)] : [],
      managerIds: filters.manager ? [Number(filters.manager)] : [],
      columns: selectedColumns,
    }
  }

  async function handleExport(format) {
    if (selectedColumns.length === 0) {
      setPageError('Отметьте хотя бы одну колонку отчёта.')
      return
    }
    setExporting(format)
    setPageError('')
    setExportNotice('')
    try {
      await downloadReport(format, buildReportPayload())
      setExportNotice(
        `Отчёт ${format.toUpperCase()} сформирован по всей выборке: ${filtered.length} строк`,
      )
    } catch (err) {
      setPageError(err.message)
    } finally {
      setExporting('')
    }
  }

  // ---------- Опции фильтров ----------

  const universityOptions = useMemo(
    () =>
      [...universities]
        .sort((a, b) => (a.shortName || a.name).localeCompare(b.shortName || b.name, 'ru'))
        .map((u) => ({ value: u.id, label: u.shortName || u.name })),
    [universities],
  )
  const directionOptions = useMemo(
    () => directions.map((d) => ({ value: d.id, label: d.name })),
    [directions],
  )
  const productOptions = useMemo(
    () => products.map((p) => ({ value: p.id, label: p.name })),
    [products],
  )
  const managerOptions = useMemo(
    () => users.map((u) => ({ value: u.id, label: userLabel(u) })),
    [users],
  )

  // ---------- Разметка ----------

  return (
    <div className="dashboard">
      <div className="page-head">
        <div>
          <h1>Отчёты</h1>
          <p className="page-sub">
            Формирование отчётов за выбранный период в форматах xls, xlsx, pdf
          </p>
        </div>
      </div>

      {pageError && (
        <div className="form-error" role="alert">
          {pageError}
        </div>
      )}
      {exportNotice && (
        <div className="form-success" role="status">
          {exportNotice}
        </div>
      )}

      {/* ---------- Панель фильтров (по макету; применение — по кнопке).
           Адаптивная сетка: поля переносятся на следующую строку,
           не сжимаются и не наслаиваются ---------- */}
      <section className="panel">
        <div className="filters-grid filters-grid--reports">
          {/* Период — календарный диапазон дат (по аннотации макета) */}
          <FilterField label="Период с">
            <input
              type="date"
              value={draftFrom}
              onChange={(e) => setDraftFrom(e.target.value)}
            />
          </FilterField>
          <FilterField label="Период по">
            <input
              type="date"
              value={draftTo}
              onChange={(e) => setDraftTo(e.target.value)}
            />
          </FilterField>
          <FilterField label="Вуз">
            <select
              value={draftUniversity}
              onChange={(e) => setDraftUniversity(e.target.value)}
            >
              <option value="">Все вузы</option>
              {universityOptions.map((o) => (
                <option key={o.value} value={String(o.value)}>
                  {o.label}
                </option>
              ))}
            </select>
          </FilterField>
          <FilterField label="ИТ-направление">
            <select
              value={draftDirection}
              onChange={(e) => setDraftDirection(e.target.value)}
            >
              <option value="">Все направления</option>
              {directionOptions.map((o) => (
                <option key={o.value} value={String(o.value)}>
                  {o.label}
                </option>
              ))}
            </select>
          </FilterField>
          <FilterField label="ИТ-продукт">
            <select
              value={draftProduct}
              onChange={(e) => setDraftProduct(e.target.value)}
            >
              <option value="">Все продукты</option>
              {productOptions.map((o) => (
                <option key={o.value} value={String(o.value)}>
                  {o.label}
                </option>
              ))}
            </select>
          </FilterField>
          {/* Фильтр «Ответственный» доступен всем ролям:
              пользователь по ТЗ формирует отчёты «по выбранным …
              ответственным за них» */}
          <FilterField label="Ответственный">
            <select
              value={draftManager}
              onChange={(e) => setDraftManager(e.target.value)}
            >
              <option value="">Все</option>
              {managerOptions.map((o) => (
                <option key={o.value} value={String(o.value)}>
                  {o.label}
                </option>
              ))}
            </select>
          </FilterField>
          <div className="filters-actions">
            <button
              className="btn-primary filters-apply"
              type="button"
              onClick={applyFilters}
            >
              Применить
            </button>
            <button className="btn-ghost" type="button" onClick={resetFilters}>
              Сбросить
            </button>
          </div>
        </div>
      </section>

      {/* ---------- Выбор колонок отчёта (по макету: галочка при выборе) ---------- */}
      <section className="panel">
        <h2 className="panel-heading">Выбор колонок отчёта</h2>
        <div className="report-columns" role="group" aria-label="Колонки отчёта">
          {ALL_COLUMNS.map((column) => {
            const checked = selectedColumns.includes(column.key)
            return (
              <label key={column.key} className="checkbox-field report-column">
                <input
                  type="checkbox"
                  checked={checked}
                  onChange={() => toggleColumn(column.key)}
                />
                {/* Галочка отрисовывается нативным чекбоксом
                    (accent-color задан в CSS), по макету — квадрат
                    с галочкой при выборе */}
                <span>{column.label}</span>
              </label>
            )
          })}
        </div>
      </section>

      {/* ---------- Предпросмотр и экспорт ---------- */}
      <section className="panel">
        <div className="panel-head">
          <div>
            <h2>Предпросмотр отчёта</h2>
            <p className="panel-sub">
              Строк в выборке: {filtered.length}
              {filters.from || filters.to
                ? ` · период: ${filters.from ? formatDate(filters.from) : '…'} — ${
                    filters.to ? formatDate(filters.to) : '…'
                  }`
                : ' · период: все время'}
            </p>
            {/* Пояснение: пагинация не влияет на состав отчёта */}
            <p className="report-export-note">
              Экспорт формируется по всей выборке ({filtered.length} строк),
              а не только по текущей странице предпросмотра.
            </p>
          </div>
          {/* Экспорт: xls/xlsx/pdf — всем ролям (требования 4 и 8 ТЗ),
              json — администратору (расширенные права, п. 11 ТЗ) */}
          <div className="report-format-row">
            {EXPORT_FORMATS.map((format) => (
              <button
                key={format.id}
                type="button"
                className="btn-accent-soft report-format-btn"
                onClick={() => handleExport(format.id)}
                disabled={exporting !== '' || selectedColumns.length === 0}
              >
                <IconDownload size={16} />
                {exporting === format.id
                  ? `Формирование ${format.label}…`
                  : `Скачать ${format.label}`}
              </button>
            ))}
            {isAdmin && (
              <button
                type="button"
                className="btn-ghost report-format-btn"
                onClick={() => handleExport(JSON_FORMAT.id)}
                disabled={exporting !== '' || selectedColumns.length === 0}
                title="Результирующий JSON-файл (для интеграций)"
              >
                <IconReport size={16} />
                {exporting === JSON_FORMAT.id
                  ? 'Формирование JSON…'
                  : 'Скачать JSON'}
              </button>
            )}
          </div>
        </div>

        {selectedColumns.length === 0 ? (
          <p className="empty">
            Отметьте хотя бы одну колонку, чтобы сформировать отчёт.
          </p>
        ) : loading ? (
          <p className="page-loader">Загрузка…</p>
        ) : filtered.length === 0 ? (
          <p className="empty">
            Нет данных по выбранным фильтрам. Измените период или сбросьте
            фильтры.
          </p>
        ) : (
          <>
            <div className="table-wrap">
              {/* Таблица предпросмотра строится строго по выбранным колонкам
                  (как и итоговый файл отчёта) */}
              <table className="report-table">
                <thead>
                  <tr>
                    {visibleColumns.map((column) => (
                      <th
                        key={column.key}
                        className={
                          column.key === 'status' ? 'report-col-status' : undefined
                        }
                      >
                        {column.label}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {paged.map((row) => (
                    <tr key={row.id}>
                      {visibleColumns.map((column) =>
                        column.key === 'status' ? (
                          // Колонке «Статус» — гарантированная ширина,
                          // чтобы пилюля занимала две строки, а не 3–4.
                          // Бейдж — span ВНУТРИ td (inline-flex на самой
                          // ячейке ломал layout таблицы).
                          <td key={column.key} className="report-col-status">
                            <span
                              className={`report-status-badge ${statusToneClass(row.status)}`}
                            >
                              {row.status}
                            </span>
                          </td>
                        ) : (
                          <td key={column.key}>{row[column.key]}</td>
                        ),
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* ---------- Пагинация предпросмотра ----------
                Только для отображения! На экспорт не влияет:
                файл формируется по всей выборке под фильтр.
                Селектор «Показывать по» виден ВСЕГДА — иначе при
                pageSize >= числу строк пропадает возможность его сменить. */}
            <nav
              className="pagination"
              aria-label="Страницы предпросмотра отчёта"
            >
              <span className="pagination-info">
                {pageCount > 1
                  ? `Показано ${shownFrom}–${shownTo} из ${filtered.length}`
                  : `Показано ${filtered.length} из ${filtered.length}`}
              </span>

              {/* Стрелки и ввод номера страницы — только если
                  страниц больше одной */}
              {pageCount > 1 && (
                <div className="pager">
                  <button
                    type="button"
                    className="page-btn"
                    onClick={() => goToPage(safePage - 1)}
                    disabled={safePage === 1}
                    aria-label="Предыдущая страница"
                    title="Предыдущая страница"
                  >
                    ‹
                  </button>
                  <span className="pager-label">Страница</span>
                  <input
                    type="text"
                    inputMode="numeric"
                    className="pager-input"
                    value={pageInput}
                    onChange={handlePageInputChange}
                    onBlur={commitPageInput}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        e.preventDefault()
                        commitPageInput()
                      }
                    }}
                    aria-label="Номер страницы"
                  />
                  <span className="pager-label">из {pageCount}</span>
                  <button
                    type="button"
                    className="page-btn"
                    onClick={() => goToPage(safePage + 1)}
                    disabled={safePage === pageCount}
                    aria-label="Следующая страница"
                    title="Следующая страница"
                  >
                    ›
                  </button>
                </div>
              )}

              {/* Выбор размера страницы — НЕ зависит от числа страниц.
                  Выбор сохраняется в localStorage (кэш действий
                  пользователя, нефункциональное требование 13) */}
              <label className="pager-size">
                <span className="pager-label">Показывать по</span>
                <select
                  className="inline-select pager-size-select"
                  value={String(pageSize)}
                  onChange={(e) => setPageSize(Number(e.target.value))}
                  aria-label="Строк на странице"
                >
                  {PAGE_SIZE_OPTIONS.map((size) => (
                    <option key={size} value={String(size)}>
                      {size}
                    </option>
                  ))}
                </select>
              </label>
            </nav>
          </>
        )}
      </section>
    </div>
  )
}