import { useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { api } from '../api/client'
import { getContractInteractionOptions, getContracts, createContract, updateContract } from '../api/contracts.js'
import { useAuth } from '../App.jsx'
import { IconPlus } from '../components/icons.jsx'

// =====================================================================
// Страница «Договоры» — реестр договоров с вузами на использование ПО.
//
// Состав:
// 1. Шапка: «Договоры» + подзаголовок, справа «Добавить договор»
//    (только manager/admin — п. 11 ТЗ).
// 2. Панель фильтров в ЕДИНОМ стиле страниц «Вузы»/«Отчёты»:
//    подпись сверху, элемент снизу, применение по кнопке «Найти»
//    (а не мгновенно). Поля: Поиск (по номеру/вузу/ПО/статусу/
//    комментарию), Подписан с / Подписан по (период), Вуз, Статус —
//    Вуз и Статус — нативные комбобоксы. Кнопки «Найти»/«Сбросить»
//    на линии полей (высота 47px, как у других страниц).
// 3. Таблица: № Договора · Вуз · ПО · Подписан · Срок · Статус.
//    Вуз и ПО агрегируются бэкендом из взаимодействий
//    (interactions.contract_id): один договор — несколько продуктов.
// 4. Клик по строке — карточка договора: manager/admin — форма
//    редактирования реквизитов; user — просмотр.
// 5. Пагинация — стандартный пейджер приложения.
// 6. Глубокая ссылка из уведомлений: ?focus={contractId} — строка
//    этого договора подсвечивается оранжевым, пагинация переходит
//    на СТРАНИЦУ, где лежит договор (по 10 строк), страница
//    прокручивается к строке. Без перехода на страницу строка не
//    рендерится — отсюда было «открывается реестр, а не тот договор».
//
// РОЛЕВАЯ МОДЕЛЬ (п. 11 ТЗ): user — просмотр; manager/admin —
// добавление и редактирование (бэкенд дополнительно проверяет
// Policies.ManagerAccess; отказ — 403).
//
// Контракт с бэкендом (ITSchoolCRM.API):
//   GET  /Contracts                 — реестр ContractDto
//   GET  /Contracts/interaction-options — взаимодействия без договора
//   POST /Contracts                 — создание (+ привязка interactionId)
//   PUT  /Contracts/{id}            — редактирование (404, если нет)
// Ошибки — ErrorResponseDto { code, message }: показываем message как есть
// (нефункциональное требование 3 ТЗ).
// =====================================================================

// ---------- Константы ----------

// Строк таблицы на страницу
const PAGE_SIZE = 10

// Статусы договора. Единый перечень фронта и форм; на бэкенде
// contracts.status — varchar(100), дефолт «На подписании»
const STATUS_OPTIONS = [
  'Черновик',
  'На подписании',
  'Активен',
  'Завершён',
  'Архивирован',
]

// ---------- Форматирование ----------

// Время приходит в UTC без суффикса Z — дописываем Z,
// чтобы браузер сделал сдвиг пояса (тот же хелпер, что у «Вузов»)
function asDate(value) {
  if (!value) return null

  const stringValue =
    typeof value === 'string' &&
    !value.endsWith('Z') &&
    !/[+-]\d{2}:?\d{2}$/.test(value)
      ? `${value}Z`
      : value

  const date = new Date(stringValue)
  return Number.isNaN(date.getTime()) ? null : date
}

function formatDate(value) {
  const date = asDate(value)
  return date ? date.toLocaleDateString('ru-RU') : '—'
}

// Колонка «Срок»: «до ДД.ММ.ГГГГ»
function formatUntil(value) {
  const date = asDate(value)
  return date ? `до ${date.toLocaleDateString('ru-RU')}` : '—'
}

// Тон бейджа статуса по ключевым словам (статус — свободный текст)
function statusClass(status) {
  const value = (status ?? '').toLowerCase()

  if (value.includes('актив')) return 'status-badge is-final'
  if (value.includes('подпис')) return 'status-badge tone-3'
  if (value.includes('заверш')) return 'status-badge tone-4'
  if (value.includes('архив')) return 'status-badge tone-5'

  return 'status-badge tone-2'
}

// Массив имён -> строка через запятую
function listText(items) {
  if (!Array.isArray(items) || items.length === 0) return '—'
  return items.join(', ')
}

// Поле фильтра: подпись сверху, элемент снизу (единый стиль проекта)
function FilterField({ label, children }) {
  return (
    <label className="field filter-field">
      <span className="field-label">{label}</span>
      {children}
    </label>
  )
}

// ---------- Главный компонент ----------

export default function ContractsPage() {
  const { user } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()

  // Ролевая модель (п. 11 ТЗ) — управляет видимостью кнопок и
  // режимом карточки (редактирование / просмотр). Фактическое право
  // всё равно проверяет бэкенд (Policies.ManagerAccess -> 403).
  const roles = user?.roles ?? []
  const canManage = roles.includes('manager') || roles.includes('admin')

  // ---------- Данные ----------

  const [contracts, setContracts] = useState([])
  const [interactionOptions, setInteractionOptions] = useState([])
  const [universities, setUniversities] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  // Подсветка строки из уведомления (?focus={contractId})
  const [highlightId, setHighlightId] = useState(null)

  // ---------- Черновики фильтров и применённые фильтры ----------

  // Черновики меняются при вводе, применённые — по кнопке «Найти»
  // (как на страницах «Вузы» и «Отчёты»)
  const [draftSearch, setDraftSearch] = useState('')
  const [draftFrom, setDraftFrom] = useState('')
  const [draftTo, setDraftTo] = useState('')
  const [draftUniversity, setDraftUniversity] = useState('')
  const [draftStatus, setDraftStatus] = useState('')
  const [filters, setFilters] = useState({
    search: '',
    from: '',
    to: '',
    university: '',
    status: '',
  })

  // ---------- Пагинация таблицы ----------

  const [page, setPage] = useState(1)

  // ---------- Модалка «Добавить договор» ----------

  const [addOpen, setAddOpen] = useState(false)
  const [addPending, setAddPending] = useState(false)
  const [addError, setAddError] = useState('')
  const [addForm, setAddForm] = useState({
    contractNumber: '',
    signedAt: '',
    validUntil: '',
    status: 'На подписании',
    interactionId: '',
    comment: '',
  })

  // ---------- Карточка договора (просмотр / редактирование) ----------

  const [detailsOpen, setDetailsOpen] = useState(false)
  const [selectedContract, setSelectedContract] = useState(null)
  const [editPending, setEditPending] = useState(false)
  const [editError, setEditError] = useState('')
  const [editForm, setEditForm] = useState({
    contractNumber: '',
    signedAt: '',
    validUntil: '',
    status: '',
    comment: '',
  })

  // ---------- Загрузка данных (один раз) ----------

  useEffect(() => {
    loadData()
  }, [])

  // Договоры + кандидаты на привязку + справочник вузов (для фильтра).
  // interaction-options — необязательный справочник: если эндпоинта нет
  // (старая версия API), договоры всё равно загружаются, селект
  // «Вуз / ПО» в модалке будет пустым.
  async function loadData() {
    setLoading(true)
    setError('')

    try {
      const [contractList, optionList, universityList] = await Promise.all([
        getContracts(api),
        getContractInteractionOptions(api).catch(() => []),
        api.get('/Universities'),
      ])

      setContracts(contractList ?? [])
      setInteractionOptions(optionList ?? [])
      setUniversities(universityList ?? [])
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  // ---------- Опции фильтра «Вуз» ----------

  const universityOptions = useMemo(
    () =>
      [...universities]
        .map((university) => ({
          value: university.id,
          label: university.shortName || university.name || `Вуз #${university.id}`,
        }))
        .sort((a, b) => a.label.localeCompare(b.label, 'ru')),
    [universities],
  )

  // ---------- Фильтрация (по кнопке «Найти») ----------

  const filtered = useMemo(() => {
    const search = filters.search.trim().toLowerCase()
    const fromTime = filters.from
      ? new Date(`${filters.from}T00:00:00`).getTime()
      : null
    const toTime = filters.to
      ? new Date(`${filters.to}T23:59:59.999`).getTime()
      : null

    return contracts.filter((contract) => {
      // Поиск: номер + вузы + продукты + статус + комментарий
      if (search) {
        const haystack = [
          contract.contractNumber,
          ...(contract.universityNames ?? []),
          ...(contract.productNames ?? []),
          contract.status,
          contract.comment,
        ]
          .filter(Boolean)
          .join(' ')
          .toLowerCase()

        if (!haystack.includes(search)) return false
      }

      // Фильтр «Вуз»: договор подходит, если хотя бы один из его вузов
      // (агрегированных из взаимодействий) совпал с выбранным
      if (filters.university) {
        const id = Number(filters.university)
        if (!(contract.universityIds ?? []).includes(id)) return false
      }

      if (filters.status && contract.status !== filters.status) {
        return false
      }

      // Период — по дате подписания, обе границы включительно
      if (fromTime !== null) {
        const signedTime = asDate(contract.signedAt)?.getTime()
        if (signedTime == null || signedTime < fromTime) return false
      }

      if (toTime !== null) {
        const signedTime = asDate(contract.signedAt)?.getTime()
        if (signedTime == null || signedTime > toTime) return false
      }

      return true
    })
  }, [contracts, filters])

  // ---------- Пагинация таблицы ----------

  const pageCount = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
  // Защита: если текущая страница стала больше числа страниц
  // (после смены фильтра) — показываем последнюю допустимую
  const safePage = Math.min(page, pageCount)

  const paged = useMemo(
    () => filtered.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE),
    [filtered, safePage],
  )

  const shownFrom = filtered.length === 0 ? 0 : (safePage - 1) * PAGE_SIZE + 1
  const shownTo = Math.min(safePage * PAGE_SIZE, filtered.length)

  // ---------- Глубокая ссылка из уведомления: ?focus={contractId} ----------

  // Приём параметра: сохраняем id договора, параметр убираем из URL,
  // чтобы обновление страницы не повторяло подсветку и скролл
  useEffect(() => {
    const focusId = searchParams.get('focus')
    if (!focusId) return
    setHighlightId(Number(focusId))
    searchParams.delete('focus')
    setSearchParams(searchParams, { replace: true })
  }, [searchParams, setSearchParams])

  // Подсветка + переход на СТРАНИЦУ, где лежит договор (пагинация
  // по 10 — без перехода строка не рендерится и подсветки не видно)
  useEffect(() => {
    if (highlightId == null || loading) return
    const idx = filtered.findIndex((c) => c.id === highlightId)
    if (idx === -1) return
    setPage(Math.floor(idx / PAGE_SIZE) + 1)
  }, [highlightId, filtered, loading])

  // Прокрутка к подсвеченной строке после того, как страница отрисовалась
  useEffect(() => {
    if (highlightId == null) return
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        document
          .querySelector('tr.row-highlight')
          ?.scrollIntoView({ behavior: 'smooth', block: 'center' })
      })
    })
  }, [highlightId, safePage, loading])

  function applyFilters() {
    setFilters({
      search: draftSearch,
      from: draftFrom,
      to: draftTo,
      university: draftUniversity,
      status: draftStatus,
    })
    setPage(1)
  }

  function resetFilters() {
    setDraftSearch('')
    setDraftFrom('')
    setDraftTo('')
    setDraftUniversity('')
    setDraftStatus('')
    setFilters({
      search: '',
      from: '',
      to: '',
      university: '',
      status: '',
    })
    setPage(1)
  }

  // ---------- Добавление договора ----------

  function openAdd() {
    setAddError('')
    setAddForm({
      contractNumber: '',
      signedAt: '',
      validUntil: '',
      status: 'На подписании',
      interactionId: '',
      comment: '',
    })
    setAddOpen(true)
  }

  // interactionId — опциональная привязка: бэкенд в одной транзакции
  // создаёт договор и проставляет interactions.contract_id, после
  // чего вуз и ПО появляются в реестре. Дублирующая привязка
  // запрещена на бэкенде (InvalidOperationException -> ERR_CONFLICT).
  async function handleAdd(event) {
    event.preventDefault()
    setAddPending(true)
    setAddError('')

    try {
      await createContract(api, {
        contractNumber: addForm.contractNumber.trim(),
        signedAt: addForm.signedAt || null,
        validUntil: addForm.validUntil || null,
        status: addForm.status,
        comment: addForm.comment.trim() || null,
        interactionId: addForm.interactionId
          ? Number(addForm.interactionId)
          : null,
      })

      setAddOpen(false)
      await loadData()
    } catch (err) {
      setAddError(err.message)
    } finally {
      setAddPending(false)
    }
  }

  // ---------- Карточка договора ----------

  // Клик по строке таблицы. Вуз и ПО в форму НЕ переносим — они
  // меняются только через привязку взаимодействий (interactions.
  // contract_id), в карточке показываются как есть (только чтение).
  function openDetails(contract) {
    setSelectedContract(contract)
    setEditError('')
    setEditForm({
      contractNumber: contract.contractNumber || '',
      signedAt: contract.signedAt ? String(contract.signedAt).slice(0, 10) : '',
      validUntil: contract.validUntil ? String(contract.validUntil).slice(0, 10) : '',
      status: contract.status || 'На подписании',
      comment: contract.comment || '',
    })
    setDetailsOpen(true)
  }

  async function handleEdit(event) {
    event.preventDefault()
    if (!selectedContract) return

    setEditPending(true)
    setEditError('')

    try {
      const updated = await updateContract(api, selectedContract.id, {
        contractNumber: editForm.contractNumber.trim(),
        signedAt: editForm.signedAt || null,
        validUntil: editForm.validUntil || null,
        status: editForm.status,
        comment: editForm.comment.trim() || null,
      })

      // Оптимистично обновляем строку без полной перезагрузки —
      // страница не «дёргается» (нефункциональное требование 2 ТЗ)
      setSelectedContract((current) => ({
        ...(current || {}),
        ...(updated || {}),
      }))
      setContracts((items) =>
        items.map((item) =>
          item.id === selectedContract.id
            ? { ...item, ...(updated || {}) }
            : item,
        ),
      )
      setDetailsOpen(false)
      setSelectedContract(null)
    } catch (err) {
      setEditError(err.message)
    } finally {
      setEditPending(false)
    }
  }

  function closeDetails() {
    setDetailsOpen(false)
    setSelectedContract(null)
  }

  // Enter в поле поиска = «Найти»
  function handleSearchKeyDown(event) {
    if (event.key === 'Enter') {
      applyFilters()
    }
  }

  // ---------- Разметка ----------

  return (
    <div className="dashboard contracts-page">
      {/* ---------- Шапка: заголовок слева, «Добавить договор» справа
      (кнопка — только руководитель/админ, п. 11 ТЗ) ---------- */}
      <div className="page-head">
        <div>
          <h1>Договоры</h1>
          <p className="page-sub">Реестр договоров по вузам и ИТ-продуктам</p>
        </div>

        {canManage && (
          <div className="page-actions">
            <button type="button" className="btn-accent-soft" onClick={openAdd}>
              <IconPlus size={16} />
              Добавить договор
            </button>
          </div>
        )}
      </div>

      {error && <div className="form-error" role="alert">{error}</div>}

      {/* ---------- Панель фильтров — в едином стиле страниц «Вузы»
      и «Отчёты»: подпись сверху, комбобоксы, применение по кнопке;
      «Найти» по высоте на линии полей ---------- */}
      <section className="panel">
        <div className="filters-grid filters-grid--contracts">
          <FilterField label="Поиск">
            <input
              type="text"
              value={draftSearch}
              onChange={(event) => setDraftSearch(event.target.value)}
              onKeyDown={handleSearchKeyDown}
              placeholder="Номер, вуз, продукт, статус…"
              aria-label="Поиск по договорам"
            />
          </FilterField>

          <FilterField label="Подписан с">
            <input
              type="date"
              value={draftFrom}
              onChange={(event) => setDraftFrom(event.target.value)}
            />
          </FilterField>

          <FilterField label="Подписан по">
            <input
              type="date"
              value={draftTo}
              onChange={(event) => setDraftTo(event.target.value)}
            />
          </FilterField>

          <FilterField label="Вуз">
            <select
              value={draftUniversity}
              onChange={(event) => setDraftUniversity(event.target.value)}
            >
              <option value="">Все вузы</option>
              {universityOptions.map((item) => (
                <option key={item.value} value={String(item.value)}>
                  {item.label}
                </option>
              ))}
            </select>
          </FilterField>

          <FilterField label="Статус">
            <select
              value={draftStatus}
              onChange={(event) => setDraftStatus(event.target.value)}
            >
              <option value="">Все статусы</option>
              {STATUS_OPTIONS.map((status) => (
                <option key={status} value={status}>{status}</option>
              ))}
            </select>
          </FilterField>

          <div className="filters-actions">
            <button
              type="button"
              className="btn-primary filters-apply"
              onClick={applyFilters}
            >
              Найти
            </button>
            <button type="button" className="btn-ghost" onClick={resetFilters}>
              Сбросить
            </button>
          </div>
        </div>
      </section>

      {/* ---------- Таблица реестра (колонки по макету) ---------- */}
      <section className="panel contracts-table-panel">
        <div className="panel-head">
          <div>
            <h2>Договоры</h2>
            <p className="panel-sub">Найдено: {filtered.length} · Нажмите на строку, чтобы открыть договор</p>
          </div>
        </div>

        {loading ? (
          <p className="page-loader">Загрузка…</p>
        ) : paged.length === 0 ? (
          <div className="contracts-empty">По заданным условиям договоры не найдены.</div>
        ) : (
          <div className="table-wrap">
            <table className="contracts-table">
              <thead>
                <tr>
                  <th>№ Договора</th>
                  <th>Вуз</th>
                  <th>ПО</th>
                  <th>Подписан</th>
                  <th>Срок</th>
                  <th>Статус</th>
                </tr>
              </thead>
              <tbody>
                {paged.map((contract) => (
                  <tr
                    key={contract.id}
                    onClick={() => openDetails(contract)}
                    className={
                      highlightId === contract.id
                        ? 'contract-row-clickable row-highlight'
                        : 'contract-row-clickable'
                    }
                    title="Нажмите, чтобы открыть договор"
                  >
                    <td className="contracts-number">{contract.contractNumber || '—'}</td>
                    <td>
                      <span className="contracts-cell-text" title={listText(contract.universityNames)}>
                        {listText(contract.universityNames)}
                      </span>
                    </td>
                    <td>
                      <span className="contracts-cell-text" title={listText(contract.productNames)}>
                        {listText(contract.productNames)}
                      </span>
                    </td>
                    <td>{formatDate(contract.signedAt)}</td>
                    <td>{formatUntil(contract.validUntil)}</td>
                    <td>
                      <span className={statusClass(contract.status)}>
                        {contract.status || '—'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* ---------- Пагинация: стандартный пейджер приложения ---------- */}
        <div className="pagination contracts-pagination">
          <div className="pagination-info">
            Показано с {shownFrom} до {shownTo} из {filtered.length}
          </div>
          <div className="pager">
            <button
              type="button"
              className="page-btn"
              disabled={safePage <= 1}
              onClick={() => setPage((value) => Math.max(1, value - 1))}
            >
              ‹
            </button>
            <span className="pager-label">
              Страница {safePage} из {pageCount}
            </span>
            <button
              type="button"
              className="page-btn"
              disabled={safePage >= pageCount}
              onClick={() => setPage((value) => Math.min(pageCount, value + 1))}
            >
              ›
            </button>
          </div>
        </div>
      </section>

      {/* ---------- Карточка договора.
      Режим определяется ролью (п. 11 ТЗ):
      manager/admin — форма редактирования; user — только просмотр.
      Закрытие по клику по подложке блокируется на время сохранения,
      чтобы случайный клик не прервал запрос ---------- */}
      {detailsOpen && selectedContract && (
        <div className="modal-overlay" onMouseDown={() => !editPending && closeDetails()}>
          {canManage ? (
            <form
              className="modal modal--wide contract-modal"
              onSubmit={handleEdit}
              onMouseDown={(event) => event.stopPropagation()}
            >
              <div className="contract-details-head">
                <div>
                  <h2>Договор {selectedContract.contractNumber || '—'}</h2>
                  <p className="form-hint">Редактирование договора</p>
                </div>
                <button
                  type="button"
                  className="btn-ghost contract-details-close"
                  onClick={closeDetails}
                  disabled={editPending}
                >
                  Закрыть
                </button>
              </div>

              <div className="modal-form">
                <label className="field">
                  <span className="field-label">№ Договора</span>
                  <input
                    type="text"
                    value={editForm.contractNumber}
                    onChange={(event) => setEditForm((form) => ({
                      ...form,
                      contractNumber: event.target.value,
                    }))}
                    required
                    disabled={editPending}
                  />
                </label>

                <label className="field">
                  <span className="field-label">Дата подписания</span>
                  <input
                    type="date"
                    value={editForm.signedAt}
                    onChange={(event) => setEditForm((form) => ({
                      ...form,
                      signedAt: event.target.value,
                    }))}
                    disabled={editPending}
                  />
                </label>

                <label className="field">
                  <span className="field-label">Срок договора (до)</span>
                  {/* min — дата подписания: срок не может быть раньше
                      (полную проверку дублирует бэкенд) */}
                  <input
                    type="date"
                    value={editForm.validUntil}
                    min={editForm.signedAt || undefined}
                    onChange={(event) => setEditForm((form) => ({
                      ...form,
                      validUntil: event.target.value,
                    }))}
                    disabled={editPending}
                  />
                </label>

                <label className="field">
                  <span className="field-label">Статус</span>
                  <select
                    value={editForm.status}
                    onChange={(event) => setEditForm((form) => ({
                      ...form,
                      status: event.target.value,
                    }))}
                    disabled={editPending}
                  >
                    {STATUS_OPTIONS.map((status) => (
                      <option key={status} value={status}>{status}</option>
                    ))}
                  </select>
                </label>

                {/* Вуз и ПО — только чтение: меняются через привязку
                    взаимодействий (interactions.contract_id), а не здесь */}
                <div className="contract-readonly-grid">
                  <div className="contract-detail-item">
                    <span>Вуз</span>
                    <strong>{listText(selectedContract.universityNames)}</strong>
                  </div>
                  <div className="contract-detail-item">
                    <span>ПО</span>
                    <strong>{listText(selectedContract.productNames)}</strong>
                  </div>
                </div>

                <label className="field">
                  <span className="field-label">Комментарий</span>
                  <textarea
                    value={editForm.comment}
                    onChange={(event) => setEditForm((form) => ({
                      ...form,
                      comment: event.target.value,
                    }))}
                    rows={4}
                    disabled={editPending}
                  />
                </label>
              </div>

              {editError && <div className="form-error" role="alert">{editError}</div>}

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn-ghost"
                  onClick={closeDetails}
                  disabled={editPending}
                >
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={editPending}>
                  {editPending ? 'Сохранение…' : 'Сохранить'}
                </button>
              </div>
            </form>
          ) : (
            /* Режим пользователя (user): карточка только для просмотра */
            <div
              className="modal modal--wide contract-modal contract-details-modal"
              onMouseDown={(event) => event.stopPropagation()}
            >
              <div className="contract-details-head">
                <div>
                  <h2>Договор {selectedContract.contractNumber || '—'}</h2>
                  <p className="form-hint">Карточка договора</p>
                </div>
                <button
                  type="button"
                  className="btn-ghost contract-details-close"
                  onClick={closeDetails}
                >
                  Закрыть
                </button>
              </div>

              <div className="contract-details-grid">
                <div className="contract-detail-item">
                  <span>№ договора</span>
                  <strong>{selectedContract.contractNumber || '—'}</strong>
                </div>
                <div className="contract-detail-item">
                  <span>Подписан</span>
                  <strong>{formatDate(selectedContract.signedAt)}</strong>
                </div>
                <div className="contract-detail-item">
                  <span>Срок договора</span>
                  <strong>{formatUntil(selectedContract.validUntil)}</strong>
                </div>
                <div className="contract-detail-item">
                  <span>Статус</span>
                  <strong>
                    <span className={statusClass(selectedContract.status)}>
                      {selectedContract.status || '—'}
                    </span>
                  </strong>
                </div>
                <div className="contract-detail-item contract-detail-item--wide">
                  <span>Вуз</span>
                  <strong>{listText(selectedContract.universityNames)}</strong>
                </div>
                <div className="contract-detail-item contract-detail-item--wide">
                  <span>ПО</span>
                  <strong>{listText(selectedContract.productNames)}</strong>
                </div>
                <div className="contract-detail-item contract-detail-item--wide">
                  <span>Комментарий</span>
                  <strong className="contract-detail-comment">{selectedContract.comment || '—'}</strong>
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {/* ---------- Модалка «Добавить договор» (manager/admin) ----------
      Селект «Вуз / ПО» — это выбор СУЩЕСТВУЮЩЕГО взаимодействия без
      договора: бэкенд в одной транзакции создаст договор и проставит
      interactions.contract_id. «Без привязки» — свободный договор
      (вуз/ПО будут «—» до ручной привязки). */}
      {addOpen && (
        <div className="modal-overlay" onMouseDown={() => !addPending && setAddOpen(false)}>
          <form
            className="modal modal--wide contract-modal"
            onSubmit={handleAdd}
            onMouseDown={(event) => event.stopPropagation()}
          >
            <h2>Добавить договор</h2>
            <p className="form-hint">
              После сохранения договор можно использовать во взаимодействиях. При выборе
              взаимодействия ВУЗ и ПО автоматически появятся в реестре.
            </p>

            <div className="modal-form">
              <label className="field">
                <span className="field-label">№ Договора</span>
                <input
                  type="text"
                  value={addForm.contractNumber}
                  onChange={(event) => setAddForm((form) => ({
                    ...form,
                    contractNumber: event.target.value,
                  }))}
                  placeholder="Например: ДГ-2026/041"
                  required
                  disabled={addPending}
                />
              </label>

              <label className="field">
                <span className="field-label">Дата подписания</span>
                <input
                  type="date"
                  value={addForm.signedAt}
                  onChange={(event) => setAddForm((form) => ({
                    ...form,
                    signedAt: event.target.value,
                  }))}
                  disabled={addPending}
                />
              </label>

              <label className="field">
                <span className="field-label">Срок договора (до)</span>
                <input
                  type="date"
                  value={addForm.validUntil}
                  min={addForm.signedAt || undefined}
                  onChange={(event) => setAddForm((form) => ({
                    ...form,
                    validUntil: event.target.value,
                  }))}
                  disabled={addPending}
                />
                <span className="field-hint">Дата окончания действия договора.</span>
              </label>

              <label className="field">
                <span className="field-label">Статус</span>
                <select
                  value={addForm.status}
                  onChange={(event) => setAddForm((form) => ({
                    ...form,
                    status: event.target.value,
                  }))}
                  disabled={addPending}
                >
                  {STATUS_OPTIONS.map((status) => (
                    <option key={status} value={status}>{status}</option>
                  ))}
                </select>
              </label>

              {/* Список кандидатов приходит с бэкенда: только
                  взаимодействия, у которых ещё нет договора
                  (interactions.contract_id IS NULL) */}
              <label className="field">
                <span className="field-label">Вуз / ПО</span>
                <select
                  value={addForm.interactionId}
                  onChange={(event) => setAddForm((form) => ({
                    ...form,
                    interactionId: event.target.value,
                  }))}
                  disabled={addPending}
                >
                  <option value="">Без привязки к взаимодействию</option>
                  {interactionOptions.map((item) => (
                    <option key={item.id} value={String(item.id)}>
                      {(item.universityName || 'Без вуза')}
                      {' — '}
                      {(item.productName || 'Без ПО')}
                    </option>
                  ))}
                </select>
                <span className="field-hint">
                  Доступны только взаимодействия, которые ещё не связаны с договором.
                </span>
              </label>

              <label className="field">
                <span className="field-label">Комментарий</span>
                <textarea
                  value={addForm.comment}
                  onChange={(event) => setAddForm((form) => ({
                    ...form,
                    comment: event.target.value,
                  }))}
                  placeholder="Необязательно"
                  rows={3}
                  disabled={addPending}
                />
              </label>
            </div>

            {addError && <div className="form-error" role="alert">{addError}</div>}

            <div className="modal-actions">
              <button
                type="button"
                className="btn-ghost"
                onClick={() => setAddOpen(false)}
                disabled={addPending}
              >
                Отмена
              </button>
              <button type="submit" className="btn-primary" disabled={addPending}>
                {addPending ? 'Сохранение…' : 'Добавить договор'}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  )
}