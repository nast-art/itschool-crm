// Страница «Ответственные» — сотрудники ИТ школы (менеджеры, закреплённые
// за вузами) и представители вузов (каталог university_contacts).
// Состав по макету:
// 1. Шапка: «Ответственные» + подзаголовок «Сотрудники ИТ школы и
//    представители вузов», справа оранжевая кнопка «+ Добавить»
//    (видна на вкладке «Представители вузов» — новые менеджеры здесь
//    не создаются: роль manager назначается администратором в Keycloak,
//    а на этой вкладке редактируется только закрепление вузов).
// 2. Вкладки «Менеджеры школы» / «Представители вузов» — активная
//    меняет цвет (пометка на макете). По умолчанию открыта «Менеджеры школы».
// 3. Таблица: ФИО · Должность · Вузы (N вузов, tooltip со списком) ·
//    Статус (Активно/Неактивно) + действия для manager/admin.
// 4. Пагинация: пейджер «‹ Страница X из Y ›» с ручным вводом номера —
//    те же классы, что на страницах «Вузы» и «Отчёты».
// 5. Модалка закрепления вузов: поиск, «Выбрать все/Снять все», список.
// 6. Модалка представителя: вуз, ФИО, должность, email, телефон,
//    тумблер активности, комментарий.
//
// АДАПТИВНОСТЬ: у каждого <td> таблицы .responsibles-table есть
// data-label с названием колонки — на мобильном (≤720px, см.
// base.css) таблица превращается в карточки. У колонки действий
// data-label нет — кнопки прижимаются к правому краю карточки.
//
// РОЛЕВАЯ МОДЕЛЬ (п. 11 ТЗ):
// user — просмотр; manager — + изменение ответственных за вузы
// (закрепление менеджеров) и управление представителями; admin — всё.
// Фактические права дублирует бэкенд (403 ERR_FORBIDDEN).
//
// Контракт с бэкендом (ITSchoolCRM.API):
// GET    /Responsibles/managers
// PUT    /Responsibles/managers/{userId}/universities
// GET    /Responsibles/contacts
// POST   /Responsibles/contacts
// PUT    /Responsibles/contacts/{id}
// DELETE /Responsibles/contacts/{id}

import { useEffect, useMemo, useState } from 'react'
import { api } from '../api/client'
import {
  createUniversityContact,
  deleteUniversityContact,
  getResponsibleManagers,
  getUniversityContacts,
  updateManagerUniversities,
  updateUniversityContact,
} from '../api/responsibles.js'
import { useAuth } from '../App.jsx'
import {
  IconBuilding,
  IconEdit,
  IconPlus,
  IconSearch,
  IconTrash,
  IconUsers,
} from '../components/icons.jsx'

const TABS = {
  managers: 'managers',
  contacts: 'contacts',
}

// Строк таблицы на страницу (пейджер, как на «Вузах» и «Отчётах»)
const PAGE_SIZE = 10

// Склонение: 1 вуз / 4 вуза / 11 вузов
function countLabel(count) {
  const n = Math.abs(Number(count) || 0)
  if (n % 10 === 1 && n % 100 !== 11) return `${count} вуз`
  if ([2, 3, 4].includes(n % 10) && ![12, 13, 14].includes(n % 100)) {
    return `${count} вуза`
  }
  return `${count} вузов`
}

function statusClass(active) {
  return active ? 'status-badge is-final' : 'status-badge tone-5'
}

function ContactForm({ form, setForm, universities, pending }) {
  return (
    <div className="responsibles-form-grid">
      <label className="field responsibles-form-full">
        <span className="field-label">Вуз</span>
        <select
          value={form.universityId}
          onChange={(e) =>
            setForm((current) => ({ ...current, universityId: e.target.value }))
          }
          disabled={pending}
          required
        >
          <option value="">Выберите вуз</option>
          {universities.map((university) => (
            <option key={university.id} value={String(university.id)}>
              {university.name || university.shortName || `Вуз #${university.id}`}
            </option>
          ))}
        </select>
      </label>

      <label className="field">
        <span className="field-label">ФИО</span>
        <input
          type="text"
          value={form.fullName}
          onChange={(e) =>
            setForm((current) => ({ ...current, fullName: e.target.value }))
          }
          placeholder="Фамилия Имя Отчество"
          disabled={pending}
          required
        />
      </label>

      <label className="field">
        <span className="field-label">Должность</span>
        <input
          type="text"
          value={form.position}
          onChange={(e) =>
            setForm((current) => ({ ...current, position: e.target.value }))
          }
          placeholder="Например: начальник отдела"
          disabled={pending}
        />
      </label>

      <label className="field">
        <span className="field-label">Email</span>
        <input
          type="email"
          value={form.email}
          onChange={(e) =>
            setForm((current) => ({ ...current, email: e.target.value }))
          }
          placeholder="name@university.ru"
          disabled={pending}
        />
      </label>

      <label className="field">
        <span className="field-label">Телефон</span>
        <input
          type="text"
          value={form.phone}
          onChange={(e) =>
            setForm((current) => ({ ...current, phone: e.target.value }))
          }
          placeholder="+7 (___) ___-__-__"
          disabled={pending}
        />
      </label>

      <div className="responsibles-toggle-row responsibles-form-full">
        <button
          type="button"
          className={form.isActive ? 'responsibles-toggle is-on' : 'responsibles-toggle'}
          onClick={() =>
            setForm((current) => ({ ...current, isActive: !current.isActive }))
          }
          disabled={pending}
          aria-pressed={form.isActive}
        >
          <span className="responsibles-toggle-dot" aria-hidden="true" />
          <span>Представитель активен</span>
        </button>
      </div>

      <label className="field responsibles-form-full">
        <span className="field-label">Комментарий</span>
        <textarea
          value={form.comment}
          onChange={(e) =>
            setForm((current) => ({ ...current, comment: e.target.value }))
          }
          placeholder="Дополнительная информация"
          rows={3}
          disabled={pending}
        />
      </label>
    </div>
  )
}

// Пикер вузов для закрепления за менеджером: поиск, выбор всех видимых,
// чекбокс-список. Используется только внутри модалки менеджера.
function UniversityPicker({ universities, selectedIds, onToggle, disabled }) {
  const [query, setQuery] = useState('')

  const filtered = useMemo(() => {
    const value = query.trim().toLowerCase()
    if (!value) return universities
    return universities.filter((university) =>
      (university.name || university.shortName || `Вуз #${university.id}`)
        .toLowerCase()
        .includes(value),
    )
  }, [universities, query])

  const selectedSet = new Set(selectedIds)
  const visibleIds = filtered.map((university) => String(university.id))
  const allVisibleSelected =
    visibleIds.length > 0 && visibleIds.every((id) => selectedSet.has(id))

  function toggleVisible() {
    if (allVisibleSelected) {
      visibleIds.forEach(onToggle)
      return
    }
    visibleIds.forEach((id) => {
      if (!selectedSet.has(id)) onToggle(id)
    })
  }

  return (
    <div className="responsibles-picker">
      <div className="responsibles-picker-toolbar">
        <div className="responsibles-picker-search">
          <IconSearch size={16} />
          <input
            type="search"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Найти вуз…"
            disabled={disabled}
          />
        </div>
        <div className="responsibles-picker-tools">
          <span className="responsibles-picker-count">
            Выбрано: {selectedIds.length}
          </span>
          <button
            type="button"
            className="responsibles-picker-link"
            onClick={toggleVisible}
            disabled={disabled || visibleIds.length === 0}
          >
            {allVisibleSelected ? 'Снять все' : 'Выбрать все'}
          </button>
        </div>
      </div>

      <div className="responsibles-picker-list">
        {filtered.length === 0 ? (
          <div className="responsibles-picker-empty">Вузов по запросу не найдено.</div>
        ) : (
          filtered.map((university) => {
            const id = String(university.id)
            const checked = selectedSet.has(id)
            return (
              <button
                key={university.id}
                type="button"
                className={
                  checked
                    ? 'responsibles-picker-item is-selected'
                    : 'responsibles-picker-item'
                }
                onClick={() => onToggle(id)}
                disabled={disabled}
                aria-pressed={checked}
              >
                <span className="responsibles-picker-check" aria-hidden="true">
                  {checked ? '✓' : ''}
                </span>
                <span className="responsibles-picker-name">
                  {university.name || university.shortName || `Вуз #${university.id}`}
                </span>
              </button>
            )
          })
        )}
      </div>
    </div>
  )
}

export default function ResponsiblesPage() {
  const { user } = useAuth()
  const roles = user?.roles ?? []
  // П. 11 ТЗ: управление ответственными — привилегия руководителя и админа
  const canManage = roles.includes('manager') || roles.includes('admin')

  const [tab, setTab] = useState(TABS.managers)
  const [managers, setManagers] = useState([])
  const [contacts, setContacts] = useState([])
  const [universities, setUniversities] = useState([])
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  // ---------- Пагинация таблицы (как на «Вузах» и «Отчётах») ----------
  const [page, setPage] = useState(1)
  const [pageInput, setPageInput] = useState('1')

  const [managerModalOpen, setManagerModalOpen] = useState(false)
  const [editingManager, setEditingManager] = useState(null)
  const [managerUniversityIds, setManagerUniversityIds] = useState([])
  const [managerSaving, setManagerSaving] = useState(false)

  const [contactModalOpen, setContactModalOpen] = useState(false)
  const [editingContact, setEditingContact] = useState(null)
  const [contactSaving, setContactSaving] = useState(false)
  const [contactForm, setContactForm] = useState({
    universityId: '',
    fullName: '',
    position: '',
    email: '',
    phone: '',
    isActive: true,
    comment: '',
  })

  function loadData() {
    setLoading(true)
    setError('')

    return Promise.all([
      getResponsibleManagers(api),
      getUniversityContacts(api),
      api.get('/Universities'),
    ])
      .then(([managerList, contactList, universityList]) => {
        setManagers(managerList ?? [])
        setContacts(contactList ?? [])
        setUniversities(universityList ?? [])
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }

  // Загрузка данных — один раз при монтировании (нефункц. требование 2:
  // страница не перезагружается, обновления — через состояния)
  useEffect(() => {
    loadData()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // ---------- Фильтрация по поиску (клиентская) ----------
  const filteredManagers = useMemo(() => {
    const value = search.trim().toLowerCase()
    if (!value) return managers

    return managers.filter((manager) => {
      const universitiesText = (manager.universities ?? [])
        .map((item) => item.name)
        .filter(Boolean)
        .join(' ')

      return [
        manager.fullName,
        universitiesText,
        manager.isActive ? 'активно' : 'неактивно',
      ]
        .filter(Boolean)
        .join(' ')
        .toLowerCase()
        .includes(value)
    })
  }, [managers, search])

  const filteredContacts = useMemo(() => {
    const value = search.trim().toLowerCase()
    if (!value) return contacts

    return contacts.filter((contact) =>
      [
        contact.fullName,
        contact.position,
        contact.universityName,
        contact.email,
        contact.phone,
        contact.isActive ? 'активно' : 'неактивно',
      ]
        .filter(Boolean)
        .join(' ')
        .toLowerCase()
        .includes(value),
    )
  }, [contacts, search])

  const currentRows = tab === TABS.managers ? filteredManagers : filteredContacts

  // Смена вкладки или поиска — возвращаемся на первую страницу
  useEffect(() => {
    setPage(1)
  }, [tab, search])

  const pageCount = Math.max(1, Math.ceil(currentRows.length / PAGE_SIZE))
  // Защита: если текущая страница стала больше числа страниц
  // (например, после смены вкладки или поиска) — последняя допустимая
  const safePage = Math.min(page, pageCount)

  const pagedRows = useMemo(
    () => currentRows.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE),
    [currentRows, safePage],
  )

  const shownFrom = currentRows.length === 0 ? 0 : (safePage - 1) * PAGE_SIZE + 1
  const shownTo = Math.min(safePage * PAGE_SIZE, currentRows.length)

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

  function openAdd() {
    setError('')

    // Новые менеджеры здесь не создаются. Роль manager назначается
    // администратором в разделе «Пользователи и права» (Keycloak),
    // а здесь редактируется только закрепление уже существующего.
    if (tab === TABS.managers) {
      return
    }

    setEditingContact(null)
    setContactForm({
      universityId: '',
      fullName: '',
      position: '',
      email: '',
      phone: '',
      isActive: true,
      comment: '',
    })
    setContactModalOpen(true)
  }

  function openManagerEdit(manager) {
    setError('')
    setEditingManager(manager)
    setManagerUniversityIds((manager.universities ?? []).map((item) => String(item.id)))
    setManagerModalOpen(true)
  }

  function closeManagerModal() {
    if (managerSaving) return
    setManagerModalOpen(false)
    setEditingManager(null)
  }

  function toggleUniversity(universityId) {
    const value = String(universityId)
    setManagerUniversityIds((current) =>
      current.includes(value)
        ? current.filter((id) => id !== value)
        : [...current, value],
    )
  }

  async function saveManagerAssignments(event) {
    event.preventDefault()
    if (!editingManager?.id) return

    setManagerSaving(true)
    setError('')

    try {
      const updated = await updateManagerUniversities(
        api,
        Number(editingManager.id),
        managerUniversityIds.map(Number),
      )

      setManagers((current) => {
        const exists = current.some((item) => item.id === updated.id)
        return exists
          ? current.map((item) => (item.id === updated.id ? updated : item))
          : [...current, updated].sort((a, b) =>
              (a.fullName ?? '').localeCompare(b.fullName ?? ''),
            )
      })
      setManagerModalOpen(false)
      setEditingManager(null)
    } catch (err) {
      setError(err.message)
    } finally {
      setManagerSaving(false)
    }
  }

  function openContactEdit(contact) {
    setError('')
    setEditingContact(contact)
    setContactForm({
      universityId: contact.universityId ? String(contact.universityId) : '',
      fullName: contact.fullName ?? '',
      position: contact.position ?? '',
      email: contact.email ?? '',
      phone: contact.phone ?? '',
      isActive: contact.isActive,
      comment: contact.comment ?? '',
    })
    setContactModalOpen(true)
  }

  async function saveContact(event) {
    event.preventDefault()
    if (!contactForm.universityId || !contactForm.fullName.trim()) return

    setContactSaving(true)
    setError('')

    const payload = {
      universityId: Number(contactForm.universityId),
      fullName: contactForm.fullName.trim(),
      position: contactForm.position.trim() || null,
      email: contactForm.email.trim() || null,
      phone: contactForm.phone.trim() || null,
      isActive: contactForm.isActive,
      comment: contactForm.comment.trim() || null,
    }

    try {
      if (editingContact) {
        const updated = await updateUniversityContact(api, editingContact.id, payload)
        setContacts((current) =>
          current.map((item) => (item.id === updated.id ? updated : item)),
        )
      } else {
        const created = await createUniversityContact(api, payload)
        setContacts((current) => [created, ...current])
      }
      setContactModalOpen(false)
    } catch (err) {
      setError(err.message)
    } finally {
      setContactSaving(false)
    }
  }

  // Мягкое удаление: бэкенд ставит is_active=false (152-ФЗ, аудит)
  async function removeContact(contact) {
    const confirmed = window.confirm(
      `Сделать представителя «${contact.fullName || 'Без ФИО'}» неактивным?`,
    )
    if (!confirmed) return

    setError('')
    try {
      await deleteUniversityContact(api, contact.id)
      setContacts((current) =>
        current.map((item) => (item.id === contact.id ? { ...item, isActive: false } : item)),
      )
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="dashboard responsibles-page">
      <div className="page-head">
        <div>
          <h1>Ответственные</h1>
          <p className="page-sub">Сотрудники ИТ школы и представители вузов</p>
        </div>

        <div className="page-actions responsibles-actions">
          {/* По макету кнопка справа. Оранжевая, как «Добавить вуз»
              и «Применить» на других страницах (общий .btn-primary).
              На вкладке менеджеров скрыта: здесь редактируется
              закрепление существующих. */}
          {canManage && tab === TABS.contacts && (
            <button type="button" className="btn-accent-soft" onClick={openAdd}>
              <IconPlus size={16} />
              Добавить
            </button>
          )}
        </div>
      </div>

      {error && (
        <div className="form-error" role="alert">
          {error}
        </div>
      )}

      <section className="panel responsibles-panel">
        <div className="responsibles-filterbar">
          {/* Вкладки по макету: активная меняет цвет */}
          <div className="responsibles-tabs" role="tablist" aria-label="Тип ответственного">
            <button
              type="button"
              role="tab"
              aria-selected={tab === TABS.managers}
              className={
                tab === TABS.managers ? 'responsibles-tab is-active' : 'responsibles-tab'
              }
              onClick={() => {
                setTab(TABS.managers)
                setSearch('')
              }}
            >
              <IconUsers size={16} />
              <span>Менеджеры школы</span>
            </button>
            <button
              type="button"
              role="tab"
              aria-selected={tab === TABS.contacts}
              className={
                tab === TABS.contacts ? 'responsibles-tab is-active' : 'responsibles-tab'
              }
              onClick={() => {
                setTab(TABS.contacts)
                setSearch('')
              }}
            >
              <IconBuilding size={16} />
              <span>Представители вузов</span>
            </button>
          </div>

          {/* Поиск: шире, плейсхолдер короткий — не обрезается */}
          <div className="responsibles-search">
            <IconSearch size={16} />
            <input
              type="search"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Поиск по ФИО или вузу"
              aria-label="Поиск ответственных"
            />
            {search && (
              <button
                type="button"
                className="responsibles-search-clear"
                onClick={() => setSearch('')}
                aria-label="Очистить поиск"
              >
                ×
              </button>
            )}
          </div>
        </div>

        <div className="responsibles-table-head">
          <div>
            <h2>{tab === TABS.managers ? 'Менеджеры школы' : 'Представители вузов'}</h2>
            <p className="panel-sub">
              {tab === TABS.managers
                ? 'Сотрудники ИТ школы и закреплённые за ними вузы'
                : 'Контакты ответственных сотрудников со стороны вузов'}
            </p>
          </div>
          <span className="responsibles-count">{currentRows.length}</span>
        </div>

        {loading ? (
          <div className="responsibles-empty">Загрузка…</div>
        ) : currentRows.length === 0 ? (
          <div className="responsibles-empty">
            <IconUsers size={30} />
            <span>Ответственные не найдены.</span>
          </div>
        ) : (
          <>
            <div className="table-wrap">
              <table className="responsibles-table">
                <thead>
                  <tr>
                    <th>ФИО</th>
                    <th>Должность</th>
                    <th>Вузы</th>
                    <th>Статус</th>
                    {canManage && <th className="responsibles-actions-col">Действия</th>}
                  </tr>
                </thead>
                <tbody>
                  {pagedRows.map((row) => (
                    <tr key={row.id}>
                      <td data-label="ФИО">
                        <strong>{row.fullName || 'Без ФИО'}</strong>
                        {row.email && (
                          <div className="responsibles-secondary">{row.email}</div>
                        )}
                      </td>
                      <td data-label="Должность">{row.position || '—'}</td>
                      <td data-label="Вузы">
                        {tab === TABS.managers ? (
                          <span
                            title={(row.universities ?? [])
                              .map((item) => item.name)
                              .join(', ')}
                          >
                            {countLabel(row.universitiesCount)}
                          </span>
                        ) : (
                          row.universityName || '—'
                        )}
                      </td>
                      <td data-label="Статус">
                        <span className={statusClass(row.isActive)}>
                          {row.isActive ? 'Активно' : 'Неактивно'}
                        </span>
                      </td>
                      {canManage && (
                        <td className="responsibles-actions-col">
                          <div className="responsibles-action-stack">
                            {tab === TABS.managers ? (
                              <button
                                type="button"
                                className="icon-action"
                                onClick={() => openManagerEdit(row)}
                                title="Изменить закреплённые вузы"
                                aria-label="Изменить закреплённые вузы"
                              >
                                <IconEdit size={16} />
                              </button>
                            ) : (
                              <>
                                <button
                                  type="button"
                                  className="icon-action"
                                  onClick={() => openContactEdit(row)}
                                  title="Изменить"
                                  aria-label="Изменить"
                                >
                                  <IconEdit size={16} />
                                </button>
                                <button
                                  type="button"
                                  className="icon-action responsibles-danger-action"
                                  onClick={() => removeContact(row)}
                                  title="Сделать неактивным"
                                  aria-label="Удалить"
                                  disabled={!row.isActive}
                                >
                                  <IconTrash size={16} />
                                </button>
                              </>
                            )}
                          </div>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* ---------- Пагинация: тот же пейджер, что на «Вузах» ---------- */}
            {pageCount > 1 && (
              <nav className="pagination" aria-label="Страницы таблицы ответственных">
                <span className="pagination-info">
                  Показано {shownFrom}–{shownTo} из {currentRows.length}
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

      {/* ---------- Модалка: закрепление вузов за менеджером ---------- */}
      {managerModalOpen && editingManager && (
        <div className="modal-overlay" onMouseDown={closeManagerModal}>
          <form
            className="modal responsibles-manager-modal"
            onSubmit={saveManagerAssignments}
            onMouseDown={(event) => event.stopPropagation()}
          >
            <h2>Изменить закрепление вузов</h2>
            <p className="form-hint">
              Здесь можно изменить только список вузов для уже назначенного менеджера.
            </p>

            <div className="responsibles-manager-summary">
              <div className="responsibles-manager-summary-icon">
                <IconUsers size={18} />
              </div>
              <div>
                <strong>{editingManager.fullName || 'Без ФИО'}</strong>
                {editingManager.email && (
                  <div className="responsibles-secondary">{editingManager.email}</div>
                )}
              </div>
            </div>

            <div className="field">
              <span className="field-label">Закреплённые вузы</span>
              <UniversityPicker
                universities={universities}
                selectedIds={managerUniversityIds}
                onToggle={toggleUniversity}
                disabled={managerSaving}
              />
            </div>

            <div className="modal-actions">
              <button
                type="button"
                className="btn-ghost"
                onClick={closeManagerModal}
                disabled={managerSaving}
              >
                Отмена
              </button>
              <button type="submit" className="btn-primary" disabled={managerSaving}>
                {managerSaving ? 'Сохранение…' : 'Сохранить'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* ---------- Модалка: добавление/редактирование представителя ---------- */}
      {contactModalOpen && (
        <div
          className="modal-overlay"
          onMouseDown={() => !contactSaving && setContactModalOpen(false)}
        >
          <form
            className="modal responsibles-contact-modal"
            onSubmit={saveContact}
            onMouseDown={(event) => event.stopPropagation()}
          >
            <h2>{editingContact ? 'Изменить представителя' : 'Добавить представителя'}</h2>
            <ContactForm
              form={contactForm}
              setForm={setContactForm}
              universities={universities}
              pending={contactSaving}
            />

            <div className="modal-actions">
              <button
                type="button"
                className="btn-ghost"
                onClick={() => setContactModalOpen(false)}
                disabled={contactSaving}
              >
                Отмена
              </button>
              <button type="submit" className="btn-primary" disabled={contactSaving}>
                {contactSaving ? 'Сохранение…' : 'Сохранить'}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  )
}