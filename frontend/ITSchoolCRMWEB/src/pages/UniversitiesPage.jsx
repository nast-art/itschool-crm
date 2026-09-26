// Страница «Вузы» — каталог учебных заведений и статусы взаимодействия.
// Состав по макету:
// 1. Шапка: «Вузы» + подзаголовок, справа кнопки «Импорт» и «Добавить вуз»
// (только руководитель и администратор — п. 11 ТЗ).
// 2. Панель фильтров: Период (дата), Вуз, ИТ-направление, Ответственный
// и кнопка «Применить» — фильтры применяются по нажатию (по макету),
// а не мгновенно.
// 3. Таблица со всей информацией (по макету и требованию 1 ТЗ):
// Название вуза · Вендор · ПО · № Договора · Подписание лицензии ·
// Срок действия · Статус передачи · Менеджер · Ответственные · Комментарий.
// 4. Действия в строке: «Назначить менеджера» (руководитель/админ).
// 5. Пагинация таблицы: пейджер «‹ Страница X из Y ›» с ручным вводом.
// 6. Глубокая ссылка из поиска в верхней панели: ?university={id} —
// страница принимает параметр и сразу применяет фильтр «Вуз»
// (работает с любой страницы приложения).
//
// РОЛЕВАЯ МОДЕЛЬ (п. 11 ТЗ):
// user — только просмотр (данные уже отфильтрованы бэкендом
// по university_managers);
// manager — просмотр + импорт + добавление вуза + назначение менеджеров
// («имеет возможность изменять ответственных пользователей
// за вузы (менять, удалять, назначать)»);
// admin — всё вышеперечисленное.
//
// АДАПТИВНОСТЬ: у каждого <td> таблицы .uni-table есть атрибут
// data-label с названием колонки — на мобильном (≤720px, см. base.css)
// таблица превращается в карточки «подпись : значение». У колонки
// действий data-label нет — кнопки просто прижимаются к правому краю.
//
// Контракт с бэкендом (ITSchoolCRM.API):
// GET /Interactions — список (уже отфильтрован по доступу)
// GET /Universities — справочник вузов
// GET /Directions — справочник направлений
// GET /Programs — программы (нужны для фильтра по направлению)
// GET /Products — продукты (колонка «Вендор»)
// GET /Contracts — договоры (колонка «№ Договора»)
// GET /Licenses — лицензии (подписание, срок, статус, комментарий)
// GET /Users — пользователи (назначение менеджера)
// PUT /Interactions/{id} — назначение менеджера (существующий эндпоинт)
// POST /Universities — добавление вуза (руководитель/админ)
// POST /Universities/import — импорт каталога xls/xlsx (multipart)
import { useEffect, useMemo, useRef, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { api } from '../api/client'
import { getContracts, getLicenses, importUniversityCatalog } from '../api/universities.js'
import { useAuth } from '../App.jsx'
import { IconPlus, IconUpload, IconUsers } from '../components/icons.jsx'

// ---------- Константы ----------

const PAGE_SIZE = 5 // строк таблицы на страницу

// Допустимые форматы импорта каталога (требование 1 ТЗ)
const IMPORT_ACCEPT = '.xls,.xlsx'

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

// Год из даты (для колонки «Срок действия» — в ТЗ указан год)
function formatYear(iso) {
  const d = asDate(iso)
  return d ? String(d.getFullYear()) : '—'
}

function userLabel(u) {
  if (!u) return '—'
  const name = [u.lastName, u.firstName, u.middleName].filter(Boolean).join(' ').trim()
  return name || u.fullName || u.email || `Пользователь #${u.id}`
}

// Тон бейджа статуса передачи лицензии
function transferStatusClass(status) {
  const s = (status ?? '').toLowerCase()
  if (s.includes('передан')) return 'status-badge is-final'
  if (s.includes('процесс')) return 'status-badge tone-3'
  if (s.includes('продлен')) return 'status-badge tone-6'
  return 'status-badge'
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

export default function UniversitiesPage() {
  const { user } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()

  // Ролевая модель (п. 11 ТЗ) — управляет видимостью кнопок и действий
  const roles = user?.roles ?? []
  const isAdmin = roles.includes('admin')
  const isManager = roles.includes('manager')
  // Руководитель и админ могут импортировать, добавлять вузы
  // и назначать менеджеров
  const canManage = isAdmin || isManager

  // Справочники
  const [interactions, setInteractions] = useState([])
  const [universities, setUniversities] = useState([])
  const [directions, setDirections] = useState([])
  const [programs, setPrograms] = useState([])
  const [products, setProducts] = useState([])
  const [users, setUsers] = useState([])
  const [contracts, setContracts] = useState([])
  const [licenses, setLicenses] = useState([])
  const [loading, setLoading] = useState(true)

  // Черновики фильтров (меняются при вводе) и применённые фильтры
  const [draftPeriod, setDraftPeriod] = useState('')
  const [draftUniversity, setDraftUniversity] = useState('')
  const [draftDirection, setDraftDirection] = useState('')
  const [draftManager, setDraftManager] = useState('')
  const [filters, setFilters] = useState({
    period: '',
    university: '',
    direction: '',
    manager: '',
  })

  // Пагинация таблицы
  const [page, setPage] = useState(1)
  const [pageInput, setPageInput] = useState('1')

  // Импорт
  const [importPending, setImportPending] = useState(false)
  const importInputRef = useRef(null)

  // Модалка «Добавить вуз»
  const [addOpen, setAddOpen] = useState(false)
  const [addPending, setAddPending] = useState(false)
  const [addError, setAddError] = useState('')
  const [addForm, setAddForm] = useState({
    name: '',
    vendor: '',
    productId: '',
    contractNumber: '',
    licenseSignedAt: '',
    licenseValidYears: '',
    transferStatus: 'В процессе передачи',
    managerId: '',
    universityContactName: '',
    comment: '',
  })

  // Модалка «Назначить менеджера»
  const [assignRow, setAssignRow] = useState(null)
  const [assignManagerId, setAssignManagerId] = useState('')
  const [assignPending, setAssignPending] = useState(false)
  const [assignError, setAssignError] = useState('')

  const [pageError, setPageError] = useState('')

  // ---------- Загрузка данных (один раз) ----------

  useEffect(() => {
    setLoading(true)
    // Договоры и лицензии — необязательные справочники: если эндпоинтов
    // ещё нет, таблица просто покажет «—» в соответствующих колонках
    Promise.all([
      api.get('/Interactions'),
      api.get('/Universities'),
      api.get('/Directions'),
      api.get('/Programs'),
      api.get('/Products'),
      api.get('/Users'),
      getContracts().catch(() => []),
      getLicenses().catch(() => []),
    ])
      .then(([list, unis, dirs, progs, prods, usrs, contr, lic]) => {
        setInteractions(list ?? [])
        setUniversities(unis ?? [])
        setDirections(dirs ?? [])
        setPrograms(progs ?? [])
        setProducts(prods ?? [])
        setUsers((usrs ?? []).filter((u) => u.isActive !== false))
        setContracts(contr ?? [])
        setLicenses(lic ?? [])
      })
      .catch((err) => setPageError(err.message))
      .finally(() => setLoading(false))
  }, [])

  function reloadInteractions() {
    return api
      .get('/Interactions')
      .then((list) => setInteractions(list ?? []))
      .catch((err) => setPageError(err.message))
  }

  // ---------- Глубокая ссылка из поиска: ?university={id} ----------
  // Поиск в верхней панели ведёт сюда с параметром. Применяем его как
  // фильтр «Вуз» (и в черновик, и в применённые фильтры — чтобы
  // таблица сразу перестроилась), затем убираем параметр из URL,
  // чтобы обновление страницы не восстанавливало фильтр повторно.
  useEffect(() => {
    const universityId = searchParams.get('university')
    if (!universityId) return
    setDraftUniversity(universityId)
    setFilters((f) => ({ ...f, university: universityId }))
    setPage(1)
    searchParams.delete('university')
    setSearchParams(searchParams, { replace: true })
  }, [searchParams, setSearchParams])

  // ---------- Сборка строк таблицы ----------

  const contractById = useMemo(
    () => Object.fromEntries(contracts.map((c) => [c.id, c])),
    [contracts],
  )
  const licenseById = useMemo(
    () => Object.fromEntries(licenses.map((l) => [l.id, l])),
    [licenses],
  )
  const productById = useMemo(
    () => Object.fromEntries(products.map((p) => [p.id, p])),
    [products],
  )
  const programById = useMemo(
    () => Object.fromEntries(programs.map((p) => [p.id, p])),
    [programs],
  )

  // Строка таблицы = взаимодействие + его договор, лицензия и продукт
  const rows = useMemo(
    () =>
      interactions.map((i) => ({
        interaction: i,
        contract: i.contractId != null ? contractById[i.contractId] : null,
        license: i.licenseId != null ? licenseById[i.licenseId] : null,
        product: i.productId != null ? productById[i.productId] : null,
      })),
    [interactions, contractById, licenseById, productById],
  )

  // ---------- Фильтрация (по кнопке «Применить») ----------

  const filtered = useMemo(() => {
    return rows.filter(({ interaction: i }) => {
      if (filters.university && i.universityId !== Number(filters.university)) {
        return false
      }
      if (filters.manager && i.managerId !== Number(filters.manager)) {
        return false
      }
      if (filters.direction) {
        const program = programById[i.programId]
        if (!program || program.directionId !== Number(filters.direction)) {
          return false
        }
      }
      if (filters.period) {
        // «Период» на макете — конкретная дата: показываем взаимодействия,
        // созданные НЕ РАНЕЕ выбранной даты
        const from = new Date(filters.period + 'T00:00:00').getTime()
        const t = i.createdAt ? new Date(i.createdAt).getTime() : 0
        if (t < from) return false
      }
      return true
    })
  }, [rows, filters, programById])

  function applyFilters() {
    setFilters({
      period: draftPeriod,
      university: draftUniversity,
      direction: draftDirection,
      manager: draftManager,
    })
    setPage(1)
  }

  function resetFilters() {
    setDraftPeriod('')
    setDraftUniversity('')
    setDraftDirection('')
    setDraftManager('')
    setFilters({ period: '', university: '', direction: '', manager: '' })
    setPage(1)
  }

  // ---------- Пагинация таблицы ----------

  const pageCount = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
  const safePage = Math.min(page, pageCount)

  const paged = useMemo(
    () => filtered.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE),
    [filtered, safePage],
  )

  const shownFrom = filtered.length === 0 ? 0 : (safePage - 1) * PAGE_SIZE + 1
  const shownTo = Math.min(safePage * PAGE_SIZE, filtered.length)

  useEffect(() => {
    setPageInput(String(safePage))
  }, [safePage])

  function goToPage(n) {
    setPage(Math.min(Math.max(1, n), pageCount))
  }

  function handlePageInputChange(e) {
    const value = e.target.value
    if (/^\d{0,3}$/.test(value)) {
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

  // ---------- Импорт каталога (требование 1 ТЗ) ----------

  async function handleImportFile(file) {
    setImportPending(true)
    setPageError('')
    try {
      await importUniversityCatalog(file)
      await reloadInteractions()
    } catch (err) {
      setPageError(err.message)
    } finally {
      setImportPending(false)
      if (importInputRef.current) importInputRef.current.value = ''
    }
  }

  // ---------- Добавление вуза ----------

  function openAdd() {
    setAddForm({
      name: '',
      vendor: '',
      productId: '',
      contractNumber: '',
      licenseSignedAt: '',
      licenseValidYears: '',
      transferStatus: 'В процессе передачи',
      managerId: '',
      universityContactName: '',
      comment: '',
    })
    setAddError('')
    setAddOpen(true)
  }

  async function handleAddSave(e) {
    e.preventDefault()
    if (!addForm.name.trim()) {
      setAddError('Введите название вуза.')
      return
    }
    setAddPending(true)
    setAddError('')
    try {
      // Поля повторяют согласованный маппинг из требования 1 ТЗ
      await api.post('/Universities', {
        name: addForm.name.trim(),
        vendor: addForm.vendor.trim() || null,
        productId: addForm.productId ? Number(addForm.productId) : null,
        contractNumber: addForm.contractNumber.trim() || null,
        licenseSignedAt: addForm.licenseSignedAt || null,
        licenseValidYears: addForm.licenseValidYears
          ? Number(addForm.licenseValidYears)
          : null,
        transferStatus: addForm.transferStatus || null,
        managerId: addForm.managerId ? Number(addForm.managerId) : null,
        universityContactName: addForm.universityContactName.trim() || null,
        comment: addForm.comment.trim() || null,
      })
      setAddOpen(false)
      await reloadInteractions()
    } catch (err) {
      setAddError(err.message)
    } finally {
      setAddPending(false)
    }
  }

  // ---------- Назначение менеджера (п. 11 ТЗ: руководитель) ----------

  function openAssign(row) {
    setAssignRow(row)
    setAssignManagerId(row.interaction.managerId ? String(row.interaction.managerId) : '')
    setAssignError('')
  }

  async function handleAssignSave(e) {
    e.preventDefault()
    if (!assignRow) return
    setAssignPending(true)
    setAssignError('')
    try {
      // Используем существующий эндпоинт редактирования взаимодействия:
      // меняем только менеджера, остальные поля передаём текущие
      await api.put(`/Interactions/${assignRow.interaction.id}`, {
        universityId: assignRow.interaction.universityId,
        programId: assignRow.interaction.programId,
        productId: assignRow.interaction.productId ?? null,
        managerId: assignManagerId ? Number(assignManagerId) : null,
      })
      setAssignRow(null)
      await reloadInteractions()
    } catch (err) {
      setAssignError(err.message)
    } finally {
      setAssignPending(false)
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
  const managerOptions = useMemo(
    () => users.map((u) => ({ value: u.id, label: userLabel(u) })),
    [users],
  )
  const productOptions = useMemo(
    () => products.map((p) => ({ value: p.id, label: p.name })),
    [products],
  )

  // ---------- Разметка ----------

  return (
    <div className="dashboard">
      <div className="page-head">
        <div>
          <h1>Вузы</h1>
          <p className="page-sub">Каталог учебных заведений и статусы взаимодействия</p>
        </div>
        {/* Импорт и добавление — только руководитель и админ (п. 11 ТЗ) */}
        {canManage && (
          <div className="page-actions">
            <button
              className="btn-ghost"
              onClick={() => importInputRef.current?.click()}
              disabled={importPending}
            >
              <IconUpload size={16} />
              {importPending ? 'Импорт…' : 'Импорт'}
            </button>
            <input
              ref={importInputRef}
              type="file"
              hidden
              accept={IMPORT_ACCEPT}
              onChange={(e) => {
                const file = e.target.files?.[0]
                if (file) handleImportFile(file)
              }}
            />
            <button className="btn-accent-soft" onClick={openAdd}>
              <IconPlus size={16} />
              Добавить вуз
            </button>
          </div>
        )}
      </div>

      {pageError && <div className="form-error" role="alert">{pageError}</div>}

      {/* ---------- Панель фильтров (по макету; применение — по кнопке) ---------- */}
      <section className="panel">
        <div className="filters-grid filters-grid--uni">
          <FilterField label="Период">
            <input
              type="date"
              value={draftPeriod}
              onChange={(e) => setDraftPeriod(e.target.value)}
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
            <button className="btn-primary filters-apply" type="button" onClick={applyFilters}>
              Применить
            </button>
            <button className="btn-ghost" type="button" onClick={resetFilters}>
              Сбросить
            </button>
          </div>
        </div>
      </section>

      {/* ---------- Таблица со всей информацией ---------- */}
      <section className="panel">
        {loading ? (
          <p className="page-loader">Загрузка…</p>
        ) : filtered.length === 0 ? (
          <p className="empty">
            Нет данных по выбранным фильтрам. Измените период или сбросьте фильтры.
          </p>
        ) : (
          <>
            <div className="table-wrap">
              <table className="uni-table">
                <thead>
                  <tr>
                    <th>Название вуза</th>
                    <th>Вендор</th>
                    <th>ПО</th>
                    <th>№ Договора</th>
                    <th>Подписание лицензии</th>
                    <th>Срок действия</th>
                    <th>Статус передачи</th>
                    <th>Менеджер</th>
                    <th>Ответственные</th>
                    <th>Комментарий</th>
                    {canManage && <th className="col-actions">Действия</th>}
                  </tr>
                </thead>
                <tbody>
                  {paged.map(({ interaction: i, contract, license, product }) => (
                    <tr key={i.id}>
                      <td className="col-university" data-label="Название вуза">
                        {i.universityName ?? '—'}
                      </td>
                      <td data-label="Вендор">{product?.vendor ?? '—'}</td>
                      <td data-label="ПО">{i.productName ?? '—'}</td>
                      <td data-label="№ Договора">{contract?.contractNumber ?? '—'}</td>
                      <td data-label="Подписание лицензии">
                        {license ? formatDate(license.signedAt ?? license.signed_at) : '—'}
                      </td>
                      <td data-label="Срок действия">
                        {license ? formatYear(license.validUntil ?? license.valid_until) : '—'}
                      </td>
                      <td data-label="Статус передачи">
                        {license?.transferStatus ?? license?.transfer_status ? (
                          <span
                            className={transferStatusClass(
                              license.transferStatus ?? license.transfer_status,
                            )}
                          >
                            {license.transferStatus ?? license.transfer_status}
                          </span>
                        ) : (
                          '—'
                        )}
                      </td>
                      <td data-label="Менеджер">{i.managerName ?? '—'}</td>
                      <td data-label="Ответственные">{i.universityContactName ?? '—'}</td>
                      <td className="col-comment" data-label="Комментарий">
                        {license?.comment ?? contract?.comment ?? '—'}
                      </td>
                      {canManage && (
                        <td className="col-actions">
                          {/* Назначение менеджера — привилегия руководителя/админа.
                              data-label нет: в карточке на мобильном кнопка
                              просто прижимается к правому краю */}
                          <button
                            type="button"
                            className="icon-btn row-action"
                            onClick={() => openAssign({ interaction: i })}
                            aria-label="Назначить менеджера"
                            title="Назначить менеджера"
                          >
                            <IconUsers size={16} />
                          </button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Пейджер таблицы */}
            {pageCount > 1 && (
              <nav className="pagination" aria-label="Страницы таблицы вузов">
                <span className="pagination-info">
                  Показано {shownFrom}–{shownTo} из {filtered.length}
                </span>
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
              </nav>
            )}
          </>
        )}
      </section>

      {/* ---------- Модалка: добавление вуза (руководитель/админ) ----------
      Поля — по согласованному маппингу требования 1 ТЗ */}
      {addOpen && (
        <div className="modal-overlay" onClick={() => setAddOpen(false)}>
          <div className="modal modal--wide" onClick={(e) => e.stopPropagation()}>
            <h2>Добавить вуз</h2>
            <form onSubmit={handleAddSave} className="modal-form">
              <label className="field">
                <span className="field-label">Название вуза</span>
                <input
                  type="text"
                  value={addForm.name}
                  onChange={(e) => setAddForm((f) => ({ ...f, name: e.target.value }))}
                  placeholder="Московский физико-технический институт"
                  required
                />
              </label>
              <div className="field-row">
                <label className="field">
                  <span className="field-label">Вендор</span>
                  <input
                    type="text"
                    value={addForm.vendor}
                    onChange={(e) => setAddForm((f) => ({ ...f, vendor: e.target.value }))}
                    placeholder="Ростелеком"
                  />
                </label>
                <label className="field">
                  <span className="field-label">ПО (продукт)</span>
                  <select
                    value={addForm.productId}
                    onChange={(e) => setAddForm((f) => ({ ...f, productId: e.target.value }))}
                  >
                    <option value="">Не выбрано</option>
                    {productOptions.map((o) => (
                      <option key={o.value} value={String(o.value)}>
                        {o.label}
                      </option>
                    ))}
                  </select>
                </label>
              </div>
              <div className="field-row">
                <label className="field">
                  <span className="field-label">№ Договора</span>
                  <input
                    type="text"
                    value={addForm.contractNumber}
                    onChange={(e) =>
                      setAddForm((f) => ({ ...f, contractNumber: e.target.value }))
                    }
                    placeholder="ДГ-2026/001"
                  />
                </label>
                <label className="field">
                  <span className="field-label">Статус передачи</span>
                  <select
                    value={addForm.transferStatus}
                    onChange={(e) =>
                      setAddForm((f) => ({ ...f, transferStatus: e.target.value }))
                    }
                  >
                    <option>В процессе передачи</option>
                    <option>Передано</option>
                    <option>Требует продления</option>
                  </select>
                </label>
              </div>
              <div className="field-row">
                <label className="field">
                  <span className="field-label">Подписание лицензии</span>
                  <input
                    type="date"
                    value={addForm.licenseSignedAt}
                    onChange={(e) =>
                      setAddForm((f) => ({ ...f, licenseSignedAt: e.target.value }))
                    }
                  />
                </label>
                <label className="field">
                  <span className="field-label">Срок действия лицензии (год)</span>
                  <input
                    type="number"
                    min="2000"
                    max="2100"
                    value={addForm.licenseValidYears}
                    onChange={(e) =>
                      setAddForm((f) => ({ ...f, licenseValidYears: e.target.value }))
                    }
                    placeholder="2027"
                  />
                </label>
              </div>
              <label className="field">
                <span className="field-label">Менеджер (ответственный от Школы)</span>
                <select
                  value={addForm.managerId}
                  onChange={(e) => setAddForm((f) => ({ ...f, managerId: e.target.value }))}
                >
                  <option value="">Не назначен</option>
                  {managerOptions.map((o) => (
                    <option key={o.value} value={String(o.value)}>
                      {o.label}
                    </option>
                  ))}
                </select>
              </label>
              <label className="field">
                <span className="field-label">Ответственные от ВУЗа</span>
                <input
                  type="text"
                  value={addForm.universityContactName}
                  onChange={(e) =>
                    setAddForm((f) => ({ ...f, universityContactName: e.target.value }))
                  }
                  placeholder="Иванов Иван Иванович"
                />
              </label>
              <label className="field">
                <span className="field-label">Комментарий</span>
                <textarea
                  value={addForm.comment}
                  onChange={(e) => setAddForm((f) => ({ ...f, comment: e.target.value }))}
                  placeholder="Дополнительная информация по вузу"
                  rows={3}
                />
              </label>
              {addError && <div className="form-error" role="alert">{addError}</div>}
              <div className="modal-actions">
                <button type="button" className="btn-ghost" onClick={() => setAddOpen(false)}>
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={addPending}>
                  {addPending ? 'Сохранение…' : 'Добавить'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ---------- Модалка: назначение менеджера (руководитель/админ) ---------- */}
      {assignRow && (
        <div className="modal-overlay" onClick={() => setAssignRow(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Назначить менеджера</h2>
            <p className="form-hint">
              Вуз: {assignRow.interaction.universityName ?? '—'}
              {assignRow.interaction.programName ? ` · ${assignRow.interaction.programName}` : ''}
            </p>
            <form onSubmit={handleAssignSave} className="modal-form">
              <label className="field">
                <span className="field-label">Менеджер</span>
                <select
                  value={assignManagerId}
                  onChange={(e) => setAssignManagerId(e.target.value)}
                >
                  <option value="">Не назначен</option>
                  {managerOptions.map((o) => (
                    <option key={o.value} value={String(o.value)}>
                      {o.label}
                    </option>
                  ))}
                </select>
              </label>
              {assignError && <div className="form-error" role="alert">{assignError}</div>}
              <div className="modal-actions">
                <button type="button" className="btn-ghost" onClick={() => setAssignRow(null)}>
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={assignPending}>
                  {assignPending ? 'Сохранение…' : 'Назначить'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}