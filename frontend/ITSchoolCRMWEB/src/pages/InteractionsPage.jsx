// Страница «Взаимодействия» — карта workflow по взаимодействию с вузом.
// Состав по макету:
//   1. Фильтры: Период, Вуз, ИТ-направление, ИТ-продукт, Ответственный
//      (фильтр «Ответственный» — только для руководителя и администратора).
//      Поддерживается вход с параметром ?direction={id} — применяет фильтр
//      по направлению (переход «Открыть workflow» со страницы ИТ-направлений).
//   2. Список взаимодействий (бэкенд отдаёт ТОЛЬКО доступные по
//      university_managers), пагинация по 6 карточек, пейджер
//      «‹ Страница 5 из 9 ›» с ручным вводом номера.
//   3. Карта этапов workflow с прогрессом и подсказками-датами.
//   4. Перевод статуса с комментарием.
//   5. Комментарии пользователей: добавление + просмотр ленты.
//   6. Файлы: drag&drop, привязка к этапу (по умолчанию шаг 1), скачивание,
//      удаление. Кнопка удаления показывается всегда — фактическое право
//      (владелец / руководитель / админ) проверяет бэкенд; при отказе
//      возвращается 403 ERR_FORBIDDEN с сообщением.
//   7. «Редактировать» и «Добавить статус» — только для admin.
//
// Контракт с бэкендом (ITSchoolCRM.API):
//   GET  /Interactions                  — список (уже отфильтрован по доступу)
//   GET  /Interactions/{id}             — базовый DTO
//   GET  /Interactions/{id}/history     — история (комментарии, даты этапов)
//   PUT  /Interactions/{id}             — редактирование карточки (admin)
//   POST /Interactions/{id}/status      — { toStatusId, comment? }
//   POST /Interactions/{id}/comments    — { comment, statusId? }
//   GET  /Attachments/interaction/{id}  — вложения
//   POST /Attachments/interaction/{id}/status/{statusId} — multipart
//   GET  /Attachments/{id}/download     — скачивание
//   DELETE /Attachments/{id}            — удаление (право решает бэкенд)
//   GET  /Workflows/{id}                — { id, name, statuses[],
//                                         transitions[] }
//   POST /Workflows/{id}/statuses       — новый этап (admin)
//   GET  /Users                         — для фильтра «Ответственный»
import { Fragment, useEffect, useMemo, useRef, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { api } from '../api/client'
import { downloadAttachment, getAttachments, uploadAttachment } from '../api/interactions.js'
import { useAuth } from '../App.jsx'
import {
  IconCheck,
  IconDownload,
  IconEdit,
  IconHandshake,
  IconPaperclip,
  IconPlus,
  IconTrash,
  IconUpload,
} from '../components/icons.jsx'

// ---------- Константы ----------

const PAGE_SIZE = 6 // сетка 3×2, как на макете

// Допустимые форматы вложений (п. 3 функциональных требований ТЗ)
const ALLOWED_EXTENSIONS = ['png', 'jpg', 'jpeg', 'pdf', 'zip', 'gz', 'gzip', 'rar', 'doc', 'docx', 'xls', 'xlsx']
const ACCEPT_ATTRIBUTE = '.png,.jpg,.jpeg,.pdf,.zip,.gz,.gzip,.rar,.doc,.docx,.xls,.xlsx'
const MAX_FILE_SIZE = 20 * 1024 * 1024 // 20 МБ на файл

const PERIODS = [
  { id: 'all', label: 'Все время' },
  { id: 'week', label: 'Неделя' },
  { id: 'month', label: 'Месяц' },
  { id: 'quarter', label: 'Квартал' },
  { id: 'year', label: 'Год' },
]

function periodStart(id) {
  if (id === 'all') return null
  const now = new Date()

  if (id === 'week') {
    const mondayOffset = (now.getDay() + 6) % 7
    return new Date(now.getFullYear(), now.getMonth(), now.getDate() - mondayOffset)
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
  return null
}

// ---------- Форматирование ----------

// Время приходит в UTC без суффикса Z (timestamp without time zone) —
// дописываем Z, чтобы браузер сделал сдвиг пояса
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

function formatDateTime(iso) {
  const d = asDate(iso)
  if (!d) return '—'
  return `${d.toLocaleDateString('ru-RU')}, ${d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })}`
}

function formatSize(bytes) {
  if (bytes == null || Number.isNaN(Number(bytes))) return ''
  const n = Number(bytes)
  if (n < 1024) return `${n} Б`
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} КБ`
  return `${(n / 1024 / 1024).toFixed(1)} МБ`
}

function userLabel(u) {
  if (!u) return '—'
  const name = [u.lastName, u.firstName, u.middleName].filter(Boolean).join(' ').trim()
  return name || u.fullName || u.email || `Пользователь #${u.id}`
}

function validateFile(file) {
  const ext = file.name.split('.').pop().toLowerCase()
  if (!ALLOWED_EXTENSIONS.includes(ext)) {
    return `Формат .${ext} не поддерживается. Допустимо: ${ALLOWED_EXTENSIONS.join(', ')}.`
  }
  if (file.size > MAX_FILE_SIZE) {
    return 'Файл больше 20 МБ. Разбейте архив или сожмите файл.'
  }
  return null
}

function statusToneClass(status) {
  if (!status) return 'status-badge'
  if (status.isFinal) return 'status-badge is-final'
  const tone = (Math.max(1, status.sortOrder) - 1) % 6 + 1
  return `status-badge tone-${tone}`
}

// ---------- Мелкие компоненты ----------

function FilterSelect({ label, value, onChange, options, placeholder = 'Все' }) {
  return (
    <label className="field filter-field">
      <span className="field-label">{label}</span>
      <select value={value} onChange={(e) => onChange(e.target.value)}>
        <option value="">{placeholder}</option>
        {options.map((o) => (
          <option key={o.value} value={String(o.value)}>
            {o.label}
          </option>
        ))}
      </select>
    </label>
  )
}

// ---------- Главный компонент ----------

export default function InteractionsPage() {
  const { user } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()

  // Роли — только для управления интерфейсом (данные фильтрует бэкенд)
  const roles = user?.roles ?? []
  const isAdmin = roles.includes('admin')
  const isPrivileged = isAdmin || roles.includes('manager')

  // Справочники
  const [universities, setUniversities] = useState([])
  const [directions, setDirections] = useState([])
  const [programs, setPrograms] = useState([])
  const [products, setProducts] = useState([])
  const [users, setUsers] = useState([])

  // Фильтры
  const [period, setPeriod] = useState('all')
  const [fUniversity, setFUniversity] = useState('')
  const [fDirection, setFDirection] = useState('')
  const [fProduct, setFProduct] = useState('')
  const [fManager, setFManager] = useState('')

  // Пагинация
  const [page, setPage] = useState(1)
  const [pageInput, setPageInput] = useState('1')

  // Список и выбранное
  const [interactions, setInteractions] = useState([])
  const [listLoading, setListLoading] = useState(true)
  const [selectedId, setSelectedId] = useState(null)

  // Детали
  const [detail, setDetail] = useState(null)
  const [detailLoading, setDetailLoading] = useState(false)
  const [workflow, setWorkflow] = useState(null)

  // Перевод статуса
  const [targetStatusId, setTargetStatusId] = useState('')
  const [transitionComment, setTransitionComment] = useState('')
  const [transitionPending, setTransitionPending] = useState(false)
  const [transitionError, setTransitionError] = useState('')

  // Комментарии
  const [commentText, setCommentText] = useState('')
  const [commentStatusId, setCommentStatusId] = useState('')
  const [commentPending, setCommentPending] = useState(false)
  const [commentError, setCommentError] = useState('')

  // Файлы
  const [selectedFile, setSelectedFile] = useState(null)
  const [attachStatusId, setAttachStatusId] = useState('')
  const [dragOver, setDragOver] = useState(false)
  const [uploadPending, setUploadPending] = useState(false)
  const [uploadError, setUploadError] = useState('')
  const [fileBusyId, setFileBusyId] = useState(null)
  const fileInputRef = useRef(null)

  // Модалки (admin)
  const [editOpen, setEditOpen] = useState(false)
  const [editPending, setEditPending] = useState(false)
  const [editError, setEditError] = useState('')
  const [editForm, setEditForm] = useState({
    universityId: '',
    directionId: '',
    programId: '',
    productId: '',
    managerId: '',
  })

  const [statusOpen, setStatusOpen] = useState(false)
  const [statusPending, setStatusPending] = useState(false)
  const [statusError, setStatusError] = useState('')
  const [statusForm, setStatusForm] = useState({
    name: '',
    description: '',
    afterStatusId: '',
    isFinal: false,
  })

  const [pageError, setPageError] = useState('')

  // ---------- Переход «Открыть workflow» со страницы ИТ-направлений:
  // применяем фильтр по направлению из URL-параметра ----------
  useEffect(() => {
    const directionId = searchParams.get('direction')
    if (directionId) {
      setFDirection(directionId)
      // Убираем параметр из URL, чтобы фильтр не «залипал»
      // при обычной навигации на страницу
      searchParams.delete('direction')
      setSearchParams(searchParams, { replace: true })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // ---------- Справочники (один раз) ----------

  useEffect(() => {
    Promise.all([
      api.get('/Universities'),
      api.get('/Directions'),
      api.get('/Programs'),
      api.get('/Products'),
    ])
      .then(([unis, dirs, progs, prods]) => {
        setUniversities(unis ?? [])
        setDirections(dirs ?? [])
        setPrograms(progs ?? [])
        setProducts(prods ?? [])
      })
      .catch((err) => setPageError(err.message))

    api.get('/Users')
      .then((list) => setUsers((list ?? []).filter((u) => u.isActive !== false)))
      .catch(() => setUsers([]))
  }, [])

  // ---------- Список взаимодействий ----------

  function loadInteractions() {
    setListLoading(true)
    return api.get('/Interactions')
      .then((list) => setInteractions(list ?? []))
      .catch((err) => setPageError(err.message))
      .finally(() => setListLoading(false))
  }

  useEffect(() => {
    loadInteractions()
  }, [])

  // ---------- Фильтрация ----------

  const programById = useMemo(
    () => Object.fromEntries(programs.map((p) => [p.id, p])),
    [programs],
  )

  const filtered = useMemo(() => {
    const from = periodStart(period)?.getTime() ?? null
    return interactions.filter((i) => {
      if (fUniversity && i.universityId !== Number(fUniversity)) return false
      if (fProduct && i.productId !== Number(fProduct)) return false
      if (isPrivileged && fManager && i.managerId !== Number(fManager)) return false
      if (fDirection) {
        const program = programById[i.programId]
        if (!program || program.directionId !== Number(fDirection)) return false
      }
      if (from != null) {
        const t = i.createdAt ? new Date(i.createdAt).getTime() : 0
        if (t < from) return false
      }
      return true
    })
  }, [interactions, period, fUniversity, fDirection, fProduct, fManager, isPrivileged, programById])

  useEffect(() => {
    if (!isPrivileged) setFManager('')
  }, [isPrivileged])

  useEffect(() => {
    setPage(1)
  }, [period, fUniversity, fDirection, fProduct, fManager])

  // ---------- Пагинация ----------

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

  useEffect(() => {
    if (filtered.length === 0) {
      setSelectedId(null)
      return
    }
    if (!filtered.some((i) => i.id === selectedId)) {
      setSelectedId(filtered[0].id)
    }
  }, [filtered, selectedId])

  // ---------- Детали ----------

  function loadDetail(id) {
    setDetailLoading(true)
    return Promise.all([
      api.get(`/Interactions/${id}`),
      api.get(`/Interactions/${id}/history`),
      getAttachments(id).catch(() => []),
    ])
      .then(([base, history, attachments]) =>
        setDetail({
          ...base,
          history: history ?? [],
          attachments: attachments ?? [],
        }),
      )
      .catch((err) => setPageError(err.message))
      .finally(() => setDetailLoading(false))
  }

  useEffect(() => {
    if (!selectedId) {
      setDetail(null)
      setWorkflow(null)
      return
    }
    loadDetail(selectedId)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedId])

  const workflowId = detail?.workflowId ?? null

  useEffect(() => {
    if (!workflowId) {
      setWorkflow(null)
      return
    }
    api.get(`/Workflows/${workflowId}`)
      .then((data) => setWorkflow(data))
      .catch(() => setWorkflow(null))
  }, [workflowId])

  const statuses = useMemo(
    () => [...(workflow?.statuses ?? [])].sort((a, b) => a.sortOrder - b.sortOrder),
    [workflow],
  )

  const currentStatus = useMemo(
    () => statuses.find((s) => s.id === detail?.currentStatusId) ?? null,
    [statuses, detail],
  )

  const currentOrder = currentStatus?.sortOrder ?? 0
  const progressPercent = statuses.length
    ? Math.round((currentOrder / statuses.length) * 100)
    : 0

  const allowedTargets = useMemo(() => {
    if (!workflow || !detail) return []
    return (workflow.transitions ?? [])
      .filter((t) => t.fromStatusId === detail.currentStatusId)
      .map((t) => statuses.find((s) => s.id === t.toStatusId))
      .filter(Boolean)
      .sort((a, b) => a.sortOrder - b.sortOrder)
  }, [workflow, detail, statuses])

  useEffect(() => {
    setTargetStatusId(allowedTargets.length ? String(allowedTargets[0].id) : '')
    setTransitionComment('')
    setTransitionError('')
  }, [allowedTargets])

  const lastChangeByStatus = useMemo(() => {
    const map = {}
    ;(detail?.history ?? []).forEach((h) => {
      const toStatusId = h.toStatusId ?? h.to_status_id
      const changedAt = h.changedAt ?? h.changed_at
      if (toStatusId != null && changedAt) {
        map[toStatusId] = changedAt
      }
    })
    return map
  }, [detail])

  const comments = useMemo(
    () =>
      (detail?.history ?? [])
        .map((h) => ({
          ...h,
          changedAt: h.changedAt ?? h.changed_at,
          changedByName: h.changedByName ?? h.changed_by_name,
          toStatusName: h.toStatusName ?? h.to_status_name,
        }))
        .filter((h) => h.comment && h.comment.trim())
        .slice()
        .sort((a, b) => new Date(b.changedAt) - new Date(a.changedAt)),
    [detail],
  )

  // ---------- Обновление после действий ----------

  async function reloadAll() {
    await Promise.all([loadInteractions(), selectedId ? loadDetail(selectedId) : Promise.resolve()])
  }

  // ---------- Перевод статуса ----------

  async function handleTransition(e) {
    e.preventDefault()
    if (!selectedId || !targetStatusId) {
      setTransitionError('Выберите статус перехода.')
      return
    }
    setTransitionPending(true)
    setTransitionError('')
    try {
      await api.post(`/Interactions/${selectedId}/status`, {
        toStatusId: Number(targetStatusId),
        comment: transitionComment.trim() || null,
      })
      setTransitionComment('')
      await reloadAll()
    } catch (err) {
      setTransitionError(err.message)
    } finally {
      setTransitionPending(false)
    }
  }

  // ---------- Комментарии ----------

  async function handleAddComment(e) {
    e.preventDefault()
    if (!selectedId || !commentText.trim()) {
      setCommentError('Введите текст комментария.')
      return
    }
    setCommentPending(true)
    setCommentError('')
    try {
      await api.post(`/Interactions/${selectedId}/comments`, {
        comment: commentText.trim(),
        statusId: commentStatusId ? Number(commentStatusId) : null,
      })
      setCommentText('')
      await reloadAll()
    } catch (err) {
      setCommentError(err.message)
    } finally {
      setCommentPending(false)
    }
  }

  // ---------- Файлы ----------

  function pickFile(file) {
    const validationError = validateFile(file)
    if (validationError) {
      setUploadError(validationError)
      setSelectedFile(null)
      return
    }
    setUploadError('')
    setSelectedFile(file)
  }

  function handleDrop(e) {
    e.preventDefault()
    setDragOver(false)
    const file = e.dataTransfer?.files?.[0]
    if (file) pickFile(file)
  }

  async function handleUpload() {
    if (!selectedId || !selectedFile) return
    setUploadPending(true)
    setUploadError('')
    try {
      await uploadAttachment(selectedId, selectedFile, attachStatusId ? Number(attachStatusId) : null)
      setSelectedFile(null)
      if (fileInputRef.current) fileInputRef.current.value = ''
      await reloadAll()
    } catch (err) {
      setUploadError(err.message)
    } finally {
      setUploadPending(false)
    }
  }

  async function handleDownload(attachment) {
    setFileBusyId(attachment.id)
    setUploadError('')
    try {
      await downloadAttachment(attachment)
    } catch (err) {
      setUploadError(err.message)
    } finally {
      setFileBusyId(null)
    }
  }

  // Удаление файла. Кнопка видна всегда; фактическое право (владелец /
  // руководитель / админ) проверяет бэкенд. При отказе вернётся
  // 403 ERR_FORBIDDEN с сообщением — оно покажется в .form-error.
  async function handleDeleteAttachment(attachment) {
    if (!window.confirm(`Удалить файл «${attachment.fileName}»? Это действие нельзя отменить.`)) return
    setFileBusyId(attachment.id)
    setUploadError('')
    try {
      await api.del(`/Attachments/${attachment.id}`)
      await reloadAll()
    } catch (err) {
      setUploadError(err.message)
    } finally {
      setFileBusyId(null)
    }
  }

  useEffect(() => {
    if (statuses.length && !attachStatusId) {
      setAttachStatusId(String(statuses[0].id))
    }
  }, [statuses, attachStatusId])

  // ---------- Редактирование карточки (admin) ----------

  const selectedUniversity = universities.find((u) => u.id === detail?.universityId)
  const editPrograms = programs.filter(
    (p) => !editForm.directionId || p.directionId === Number(editForm.directionId),
  )

  function openEdit() {
    if (!detail) return
    const program = programById[detail.programId]
    setEditForm({
      universityId: detail.universityId ? String(detail.universityId) : '',
      directionId: program?.directionId ? String(program.directionId) : '',
      programId: detail.programId ? String(detail.programId) : '',
      productId: detail.productId ? String(detail.productId) : '',
      managerId: detail.managerId ? String(detail.managerId) : '',
    })
    setEditError('')
    setEditOpen(true)
  }

  async function handleEditSave(e) {
    e.preventDefault()
    if (!selectedId) return
    if (!editForm.universityId || !editForm.programId) {
      setEditError('Заполните обязательные поля: вуз и программа.')
      return
    }
    setEditPending(true)
    setEditError('')
    try {
      await api.put(`/Interactions/${selectedId}`, {
        universityId: Number(editForm.universityId),
        programId: Number(editForm.programId),
        productId: editForm.productId ? Number(editForm.productId) : null,
        managerId: editForm.managerId ? Number(editForm.managerId) : null,
      })
      setEditOpen(false)
      await reloadAll()
    } catch (err) {
      setEditError(err.message)
    } finally {
      setEditPending(false)
    }
  }

  // ---------- Добавление статуса в workflow (admin) ----------

  function openAddStatus() {
    if (!workflow) return
    setStatusForm({
      name: '',
      description: '',
      afterStatusId: statuses.length ? String(statuses[statuses.length - 1].id) : '',
      isFinal: false,
    })
    setStatusError('')
    setStatusOpen(true)
  }

  async function handleStatusSave(e) {
    e.preventDefault()
    if (!workflow) return
    if (!statusForm.name.trim()) {
      setStatusError('Введите название статуса.')
      return
    }
    setStatusPending(true)
    setStatusError('')
    try {
      await api.post(`/Workflows/${workflow.id}/statuses`, {
        name: statusForm.name.trim(),
        description: statusForm.description.trim() || null,
        insertAfterStatusId: statusForm.afterStatusId ? Number(statusForm.afterStatusId) : null,
        isFinal: statusForm.isFinal,
      })
      setStatusOpen(false)
      const fresh = await api.get(`/Workflows/${workflow.id}`)
      setWorkflow(fresh)
    } catch (err) {
      setStatusError(err.message)
    } finally {
      setStatusPending(false)
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

  const attachments = detail?.attachments ?? []
  const isFinalReached = Boolean(currentStatus?.isFinal)

  // ---------- Разметка ----------

  return (
    <div className="dashboard">
      <div className="page-head">
        <div>
          <h1>Взаимодействия</h1>
          <p className="page-sub">Карта взаимодействия с вузом по workflow</p>
        </div>
        {isAdmin && (
          <div className="page-actions">
            <button className="btn-ghost" onClick={openAddStatus} disabled={!workflow}>
              <IconPlus size={16} />
              Добавить статус
            </button>
            <button className="btn-accent-soft" onClick={openEdit} disabled={!detail}>
              <IconEdit size={16} />
              Редактировать
            </button>
          </div>
        )}
      </div>

      {pageError && <div className="form-error" role="alert">{pageError}</div>}

      {/* ---------- Фильтры ---------- */}
      <section className="panel">
        <div className="filters-grid">
          <FilterSelect
            label="Период"
            value={period}
            onChange={setPeriod}
            options={PERIODS.map((p) => ({ value: p.id, label: p.label }))}
            placeholder="Выберите период"
          />
          <FilterSelect
            label="Вуз"
            value={fUniversity}
            onChange={setFUniversity}
            options={universityOptions}
          />
          <FilterSelect
            label="ИТ-направление"
            value={fDirection}
            onChange={setFDirection}
            options={directionOptions}
          />
          <FilterSelect
            label="ИТ-продукт"
            value={fProduct}
            onChange={setFProduct}
            options={productOptions}
          />
          {isPrivileged && (
            <FilterSelect
              label="Ответственный"
              value={fManager}
              onChange={setFManager}
              options={managerOptions}
            />
          )}
        </div>
      </section>

      {/* ---------- Список + пагинация ---------- */}
      {listLoading ? (
        <p className="page-loader">Загрузка…</p>
      ) : filtered.length === 0 ? (
        <section className="panel">
          <p className="empty">
            Нет взаимодействий по выбранным фильтрам. Измените период или сбросьте фильтры.
          </p>
        </section>
      ) : (
        <>
          <div className="interaction-cards">
            {paged.map((i) => {
              const program = programById[i.programId]
              return (
                <button
                  key={i.id}
                  type="button"
                  className={i.id === selectedId ? 'interaction-card active' : 'interaction-card'}
                  onClick={() => setSelectedId(i.id)}
                >
                  <div className="ic-head">
                    <strong>{i.universityName ?? selectedUniversity?.name ?? `Вуз #${i.universityId}`}</strong>
                    {i.currentStatusName && (
                      <span className="status-badge" title={i.currentStatusName}>
                        {i.currentStatusName}
                      </span>
                    )}
                  </div>
                  <div className="ic-meta">
                    <span>{program?.name ?? i.programName ?? '—'}</span>
                    <span>{i.productName ?? '—'}</span>
                    <span>{i.managerName ?? 'Ответственный не назначен'}</span>
                    <span>создано {formatDate(i.createdAt)}</span>
                  </div>
                </button>
              )
            })}
          </div>

          {pageCount > 1 && (
            <nav className="pagination" aria-label="Страницы списка взаимодействий">
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

      {/* ---------- Карта выбранного взаимодействия ---------- */}
      {selectedId && (
        <section className="panel">
          <div className="panel-head">
            <div>
              <h2>Взаимодействие с ВУЗом</h2>
              <p className="panel-sub">
                {detail?.universityName ?? selectedUniversity?.name ?? '—'}
                {' · '}
                {detail?.managerName ?? 'ответственный не назначен'}
              </p>
            </div>
            {currentStatus && (
              <span className={statusToneClass(currentStatus)}>{currentStatus.name}</span>
            )}
          </div>

          {detailLoading || !workflow ? (
            <p className="page-loader">Загрузка карты взаимодействия…</p>
          ) : (
            <>
              <div className="workflow-map">
                <div className="wf-track">
                  {statuses.map((s, idx) => {
                    const done = s.sortOrder <= currentOrder
                    const isCurrent = s.id === detail.currentStatusId
                    const changedAt = lastChangeByStatus[s.id]

                    let tip
                    if (changedAt) {
                      tip = `Обновлено: ${formatDate(changedAt)}`
                    } else if (done || isCurrent) {
                      tip = 'Дата изменения не зафиксирована'
                    } else {
                      tip = 'Этап ещё не пройден'
                    }

                    const stepClass = [
                      'wf-step',
                      done ? 'done' : '',
                      isCurrent ? 'current' : '',
                    ].filter(Boolean).join(' ')

                    return (
                      <Fragment key={s.id}>
                        <div className={stepClass}>
                          <div className="wf-node" data-tip={tip}>
                            {done ? <IconCheck /> : <IconHandshake />}
                          </div>
                          <span className="wf-step-name">{s.name}</span>
                        </div>
                        {idx < statuses.length - 1 && <span className="wf-connector" />}
                      </Fragment>
                    )
                  })}
                </div>
              </div>

              <div className="wf-progress">
                <div className="wf-progress-track">
                  <div className="wf-progress-fill" style={{ width: `${progressPercent}%` }} />
                </div>
                <span className="wf-progress-label">
                  {statuses.length} этапов · Пройдено {currentOrder}/{statuses.length}
                </span>
              </div>

              {!isFinalReached && allowedTargets.length > 0 && (
                <form className="transition-box" onSubmit={handleTransition}>
                  <h3>Перевод статуса</h3>
                  <label className="field">
                    <span className="field-label">Новый статус</span>
                    <select
                      value={targetStatusId}
                      onChange={(e) => setTargetStatusId(e.target.value)}
                    >
                      {allowedTargets.map((s) => (
                        <option key={s.id} value={String(s.id)}>
                          Шаг {s.sortOrder} «{s.name}»
                        </option>
                      ))}
                    </select>
                  </label>
                  <label className="field">
                    <span className="field-label">Комментарий к переходу</span>
                    <textarea
                      value={transitionComment}
                      onChange={(e) => setTransitionComment(e.target.value)}
                      placeholder="Например: передали пакет документов на согласование"
                      rows={3}
                    />
                  </label>
                  {transitionError && (
                    <div className="form-error" role="alert">{transitionError}</div>
                  )}
                  <div>
                    <button className="btn-primary" type="submit" disabled={transitionPending}>
                      {transitionPending ? 'Перевод…' : 'Перевести статус'}
                    </button>
                  </div>
                </form>
              )}
              {isFinalReached && (
                <p className="empty">Взаимодействие завершено (достигнут финальный статус).</p>
              )}
            </>
          )}
        </section>
      )}

      {/* ---------- Комментарии и файлы ---------- */}
      {selectedId && detail && (
        <section className="interactions-grid">
          {/* Комментарии пользователей */}
          <div className="panel">
            <h2>Комментарии пользователей</h2>
            <div className="comments-list">
              {comments.length === 0 ? (
                <p className="empty">Комментариев пока нет — будьте первым.</p>
              ) : (
                comments.map((c) => (
                  <div className="comment" key={c.id}>
                    <span className="comment-avatar">
                      {(c.changedByName ?? 'С').trim().charAt(0).toUpperCase()}
                    </span>
                    <div className="comment-body">
                      <div className="comment-meta">
                        <strong>{c.changedByName ?? 'Система'}</strong>
                        <span>{formatDateTime(c.changedAt)}</span>
                        {c.toStatusName && (
                          <span className="status-badge">{c.toStatusName}</span>
                        )}
                      </div>
                      <p>{c.comment}</p>
                    </div>
                  </div>
                ))
              )}
            </div>
            <form className="comment-form" onSubmit={handleAddComment}>
              <textarea
                value={commentText}
                onChange={(e) => setCommentText(e.target.value)}
                placeholder="Написать комментарий…"
                rows={3}
              />
              <div className="comment-form-actions">
                <select
                  className="inline-select"
                  value={commentStatusId}
                  onChange={(e) => setCommentStatusId(e.target.value)}
                  title="Привязка комментария к этапу (необязательно)"
                >
                  <option value="">Без привязки к этапу</option>
                  {statuses.map((s) => (
                    <option key={s.id} value={String(s.id)}>
                      Шаг {s.sortOrder} «{s.name}»
                    </option>
                  ))}
                </select>
                <button className="btn-primary" type="submit" disabled={commentPending}>
                  {commentPending ? 'Отправка…' : 'Добавить комментарий'}
                </button>
              </div>
              {commentError && <div className="form-error" role="alert">{commentError}</div>}
            </form>
          </div>

          {/* Файлы */}
          <div className="panel">
            <h2>Файлы</h2>
            <div
              className={dragOver ? 'dropzone drag-over' : 'dropzone'}
              onClick={() => fileInputRef.current?.click()}
              onDragOver={(e) => {
                e.preventDefault()
                setDragOver(true)
              }}
              onDragLeave={() => setDragOver(false)}
              onDrop={handleDrop}
              role="button"
              tabIndex={0}
              onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') fileInputRef.current?.click()
              }}
            >
              <IconUpload />
              <p>Перетащите файл сюда или нажмите для выбора</p>
              <p className="form-hint">
                png, jpeg, pdf, zip, gzip, rar, doc, docx, xls, xlsx · до 20 МБ
              </p>
              <input
                ref={fileInputRef}
                type="file"
                hidden
                accept={ACCEPT_ATTRIBUTE}
                onChange={(e) => {
                  const file = e.target.files?.[0]
                  if (file) pickFile(file)
                }}
              />
            </div>

            {selectedFile && (
              <div className="selected-file">
                <span>
                  {selectedFile.name} · {formatSize(selectedFile.size)}
                </span>
                <button
                  type="button"
                  className="btn-ghost"
                  onClick={() => {
                    setSelectedFile(null)
                    if (fileInputRef.current) fileInputRef.current.value = ''
                  }}
                >
                  Убрать
                </button>
              </div>
            )}

            <label className="field">
              <span className="field-label">К какому статусу привязать?</span>
              <select
                value={attachStatusId}
                onChange={(e) => setAttachStatusId(e.target.value)}
              >
                {statuses.map((s) => (
                  <option key={s.id} value={String(s.id)}>
                    Шаг {s.sortOrder} «{s.name}»
                  </option>
                ))}
              </select>
            </label>

            <div className="upload-actions">
              <button
                className="btn-primary"
                type="button"
                onClick={handleUpload}
                disabled={!selectedFile || uploadPending}
              >
                {uploadPending ? 'Загрузка…' : 'Загрузить'}
              </button>
            </div>

            {uploadError && <div className="form-error" role="alert">{uploadError}</div>}

            <ul className="file-list">
              {attachments.length === 0 ? (
                <li className="empty">Файлы ещё не прикладывались.</li>
              ) : (
                attachments.map((a) => (
                  <li className="file-item" key={a.id}>
                    <IconPaperclip />
                    <span className="file-name" title={a.fileName}>
                      {a.fileName}
                    </span>
                    <span className="file-meta">
                      {a.statusName ?? ''}
                      {a.statusName ? ' · ' : ''}
                      {formatDate(a.createdAt)}
                      {a.fileSize != null ? ` · ${formatSize(a.fileSize)}` : ''}
                    </span>
                    <button
                      type="button"
                      className="icon-btn file-download"
                      onClick={() => handleDownload(a)}
                      disabled={fileBusyId === a.id}
                      aria-label={`Скачать ${a.fileName}`}
                      title="Скачать"
                    >
                      {fileBusyId === a.id ? '…' : <IconDownload size={18} />}
                    </button>
                    {/* Кнопка удаления видна всегда; право проверяет бэкенд */}
                    <button
                      type="button"
                      className="icon-btn file-download file-delete"
                      onClick={() => handleDeleteAttachment(a)}
                      disabled={fileBusyId === a.id}
                      aria-label={`Удалить ${a.fileName}`}
                      title="Удалить"
                    >
                      <IconTrash size={16} />
                    </button>
                  </li>
                ))
              )}
            </ul>
          </div>
        </section>
      )}

      {/* ---------- Модалка: редактирование взаимодействия (admin) ---------- */}
      {editOpen && (
        <div className="modal-overlay" onClick={() => setEditOpen(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Редактирование взаимодействия</h2>
            <form onSubmit={handleEditSave} className="modal-form">
              <label className="field">
                <span className="field-label">Вуз</span>
                <select
                  value={editForm.universityId}
                  onChange={(e) => setEditForm((f) => ({ ...f, universityId: e.target.value }))}
                  required
                >
                  <option value="">Выберите вуз</option>
                  {universityOptions.map((o) => (
                    <option key={o.value} value={String(o.value)}>{o.label}</option>
                  ))}
                </select>
              </label>
              <label className="field">
                <span className="field-label">ИТ-направление</span>
                <select
                  value={editForm.directionId}
                  onChange={(e) =>
                    setEditForm((f) => ({ ...f, directionId: e.target.value, programId: '' }))
                  }
                >
                  <option value="">Выберите направление</option>
                  {directionOptions.map((o) => (
                    <option key={o.value} value={String(o.value)}>{o.label}</option>
                  ))}
                </select>
              </label>
              <label className="field">
                <span className="field-label">ИТ-программа</span>
                <select
                  value={editForm.programId}
                  onChange={(e) => setEditForm((f) => ({ ...f, programId: e.target.value }))}
                  required
                >
                  <option value="">Выберите программу</option>
                  {editPrograms.map((p) => (
                    <option key={p.id} value={String(p.id)}>{p.name}</option>
                  ))}
                </select>
              </label>
              <label className="field">
                <span className="field-label">ИТ-продукт</span>
                <select
                  value={editForm.productId}
                  onChange={(e) => setEditForm((f) => ({ ...f, productId: e.target.value }))}
                >
                  <option value="">Без продукта</option>
                  {productOptions.map((o) => (
                    <option key={o.value} value={String(o.value)}>{o.label}</option>
                  ))}
                </select>
              </label>
              <label className="field">
                <span className="field-label">Ответственный</span>
                <select
                  value={editForm.managerId}
                  onChange={(e) => setEditForm((f) => ({ ...f, managerId: e.target.value }))}
                >
                  <option value="">Не назначен</option>
                  {managerOptions.map((o) => (
                    <option key={o.value} value={String(o.value)}>{o.label}</option>
                  ))}
                </select>
              </label>
              {editError && <div className="form-error" role="alert">{editError}</div>}
              <div className="modal-actions">
                <button type="button" className="btn-ghost" onClick={() => setEditOpen(false)}>
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={editPending}>
                  {editPending ? 'Сохранение…' : 'Сохранить'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ---------- Модалка: добавление статуса в workflow (admin) ---------- */}
      {statusOpen && (
        <div className="modal-overlay" onClick={() => setStatusOpen(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Добавить статус в workflow</h2>
            <p className="form-hint">
              Workflow: {workflow?.name ?? '—'}. Новый этап появится на карте всех взаимодействий
              этого workflow.
            </p>
            <form onSubmit={handleStatusSave} className="modal-form">
              <label className="field">
                <span className="field-label">Название статуса</span>
                <input
                  type="text"
                  value={statusForm.name}
                  onChange={(e) => setStatusForm((f) => ({ ...f, name: e.target.value }))}
                  placeholder="Например: Адаптация программы под вуз"
                  required
                />
              </label>
              <label className="field">
                <span className="field-label">Описание (необязательно)</span>
                <textarea
                  value={statusForm.description}
                  onChange={(e) => setStatusForm((f) => ({ ...f, description: e.target.value }))}
                  placeholder="Что происходит на этом этапе"
                  rows={3}
                />
              </label>
              <label className="field">
                <span className="field-label">Вставить после этапа</span>
                <select
                  value={statusForm.afterStatusId}
                  onChange={(e) => setStatusForm((f) => ({ ...f, afterStatusId: e.target.value }))}
                >
                  <option value="">В конец</option>
                  {statuses.map((s) => (
                    <option key={s.id} value={String(s.id)}>
                      Шаг {s.sortOrder} «{s.name}»
                    </option>
                  ))}
                </select>
              </label>
              <label className="checkbox-field">
                <input
                  type="checkbox"
                  checked={statusForm.isFinal}
                  onChange={(e) => setStatusForm((f) => ({ ...f, isFinal: e.target.checked }))}
                />
                Финальный статус (завершение взаимодействия)
              </label>
              {statusError && <div className="form-error" role="alert">{statusError}</div>}
              <div className="modal-actions">
                <button type="button" className="btn-ghost" onClick={() => setStatusOpen(false)}>
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={statusPending}>
                  {statusPending ? 'Добавление…' : 'Добавить статус'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}