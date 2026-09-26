// Страница «Пользователи и права доступа» — раздел «Система»,
// доступен только администраторам (п. 11 ТЗ: «Администратор —
// расширенные права для реализации дополнительных настроек
// и управления правами пользователей, а также разграничение
// пользователей по доступу к данным»).
//
// Макет: ДВЕ таблицы одна под другой, обе видны всегда —
//   1) «Пользователи»: ФИО · Роль · Статус · Действия
//   2) «Права доступа, выбранного пользователя»:
//      Раздел · Просмотр · Изменение · Удаление
// Переключатель таблиц намеренно НЕ используется: состав по макету,
// на мобильном каждая таблица сама превращается в карточки.
//
// АДАПТИВНОСТЬ: у каждого <td> таблиц .access-users-table и
// .access-rights-table есть data-label с названием колонки — на
// мобильном (≤720px, см. base.css) таблицы превращаются в карточки
// «подпись : значение». У колонки действий data-label нет.
//
// Пагинация таблицы пользователей: 5 строк на страницу (как в
// «Вузах»), пейджер «‹ Страница X из Y ›» с ручным вводом номера.
//
// Контракт с бэкендом (ITSchoolCRM.API):
//   GET /Users/access-control — список пользователей с ролями
//     (агрегат Keycloak + таблица users; каждый ответ:
//     { keycloakUserId, userName, email, lastName, firstName,
//       middleName, fullName, role, isActive })
//   PUT /Users/{keycloakUserId}/access-control — смена роли
//     и блокировка: { role, isActive } → обновлённая запись
// Ошибки приходят в ErrorResponseDto { code, message } — клиент
// (api/client.js) уже их разбирает и пробрасывает message.
//
// Ролевая модель (п. 11 ТЗ):
//   user — работа с данными в рамках прав;
//   manager — то же + управление ответственными за вузы;
//   admin — всё вышеперечисленное + этот раздел.
// Свою учётную запись редактировать нельзя (проверка и на фронте,
// и на бэкенде — иначе админ мог бы лишить себя последней роли).
//
// Нефункциональное требование 13 (кэш действий пользователя):
// строка поиска сохраняется в localStorage
// и восстанавливается между сессиями.

import { useEffect, useMemo, useState } from 'react'
import { api } from '../api/client'
import { useAuth } from '../App.jsx'
import { IconEdit, IconUsers } from '../components/icons.jsx'

// ---------- Константы ----------

// Строк таблицы на страницу — как на странице «Вузы»
const PAGE_SIZE = 5

const ROLE_LABELS = {
  user: 'Пользователь',
  manager: 'Руководитель',
  admin: 'Администратор',
}

// Матрица прав по разделам CRM — строго по п. 11 ТЗ.
// view/edit/remove — роли, которым операция разрешена.
const ACCESS_ROWS = [
  {
    section: 'Главная',
    view: ['user', 'manager', 'admin'],
    edit: [],
    remove: [],
  },
  {
    section: 'Взаимодействия',
    view: ['user', 'manager', 'admin'],
    // Перевод статуса, комментарии, файлы — все роли (функц. требования 2–3)
    edit: ['user', 'manager', 'admin'],
    remove: [],
  },
  {
    section: 'Вузы',
    view: ['user', 'manager', 'admin'],
    // Добавление/импорт — руководитель и администратор (п. 11 ТЗ)
    edit: ['manager', 'admin'],
    remove: ['admin'],
  },
  {
    section: 'ИТ-направления',
    view: ['user', 'manager', 'admin'],
    edit: ['manager', 'admin'],
    remove: ['admin'],
  },
  {
    section: 'Ответственные',
    view: ['user', 'manager', 'admin'],
    // Руководитель назначает/меняет ответственных за вузы (п. 11 ТЗ)
    edit: ['manager', 'admin'],
    remove: [],
  },
  {
    section: 'Договоры',
    view: ['user', 'manager', 'admin'],
    edit: [],
    remove: [],
  },
  {
    section: 'Отчёты',
    view: ['user', 'manager', 'admin'],
    edit: [],
    remove: [],
  },
  {
    section: 'Визуализация',
    view: ['user', 'manager', 'admin'],
    edit: [],
    remove: [],
  },
  {
    section: 'Документация',
    view: ['user', 'manager', 'admin'],
    edit: [],
    remove: [],
  },
  {
    section: 'Система',
    view: ['admin'],
    edit: ['admin'],
    remove: ['admin'],
  },
]

// Ключ localStorage — кэш действий пользователя (требование 13):
// сохраняется только строка поиска (переключатель таблиц убран)
const SEARCH_STORAGE_KEY = 'users_access_search'

// ---------- Хелперы ----------

// ФИО из трёх частей (формат таблицы users после миграции:
// full_name удалён, есть last_name/first_name/middle_name)
function fullName(item) {
  const composed = [item.lastName, item.firstName, item.middleName]
    .filter(Boolean)
    .join(' ')
    .trim()
  return composed || item.fullName || item.userName || item.email || 'Без имени'
}

// Галочка/тире в матрице прав
function AccessMark({ allowed }) {
  return (
    <span
      className={allowed ? 'access-mark access-mark--yes' : 'access-mark'}
      aria-label={allowed ? 'Разрешено' : 'Нет доступа'}
      title={allowed ? 'Разрешено' : 'Нет доступа'}
    >
      {allowed ? '✓' : '—'}
    </span>
  )
}

// ---------- Главный компонент ----------

export default function UsersAccessPage() {
  const { user: currentUser } = useAuth()

  // Список пользователей (из Keycloak + таблица users)
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  // Выбранный пользователь (по keycloakUserId) — его права показывает
  // нижняя таблица. null = никто не выбран.
  const [selectedId, setSelectedId] = useState(null)

  // Поиск по списку пользователей (кэшируется, требование 13)
  const [search, setSearch] = useState(
    () => localStorage.getItem(SEARCH_STORAGE_KEY) ?? '',
  )

  // Пагинация таблицы пользователей
  const [page, setPage] = useState(1)
  const [pageInput, setPageInput] = useState('1')

  // Модалка редактирования роли/статуса
  const [editUser, setEditUser] = useState(null)
  const [editRole, setEditRole] = useState('user')
  const [editActive, setEditActive] = useState(true)
  const [saving, setSaving] = useState(false)

  // ---------- Загрузка списка ----------

  useEffect(() => {
    setLoading(true)
    api
      .get('/Users/access-control')
      .then((list) => {
        const next = list ?? []
        setUsers(next)
        // Если выбранный пользователь пропал из списка — сбрасываем
        setSelectedId((current) =>
          next.some((u) => u.keycloakUserId === current)
            ? current
            : (next[0]?.keycloakUserId ?? null),
        )
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  // ---------- Кэш действий пользователя (требование 13) ----------

  useEffect(() => {
    localStorage.setItem(SEARCH_STORAGE_KEY, search)
  }, [search])

  // ---------- Фильтрация поиска ----------

  const filteredUsers = useMemo(() => {
    const value = search.trim().toLowerCase()
    if (!value) return users

    return users.filter((item) => {
      const haystack = [
        fullName(item),
        item.email,
        item.userName,
        ROLE_LABELS[item.role] ?? item.role,
      ]
        .filter(Boolean)
        .join(' ')
        .toLowerCase()
      return haystack.includes(value)
    })
  }, [search, users])

  const selectedUser =
    users.find((item) => item.keycloakUserId === selectedId) ?? null

  const isCurrentUser = (item) =>
    item.keycloakUserId === currentUser?.keycloakUserId

  // ---------- Пагинация ----------

  // Новый поиск — возврат на первую страницу
  useEffect(() => {
    setPage(1)
  }, [search])

  const pageCount = Math.max(1, Math.ceil(filteredUsers.length / PAGE_SIZE))
  // Защита: если текущая страница стала больше числа страниц
  // (например, после изменения поиска) — показываем последнюю допустимую
  const safePage = Math.min(page, pageCount)

  const pagedUsers = useMemo(
    () => filteredUsers.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE),
    [filteredUsers, safePage],
  )

  const shownFrom = filteredUsers.length === 0 ? 0 : (safePage - 1) * PAGE_SIZE + 1
  const shownTo = Math.min(safePage * PAGE_SIZE, filteredUsers.length)

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

  // ---------- Редактирование ----------

  function openEdit(item) {
    if (isCurrentUser(item)) return // свою запись менять нельзя
    setEditUser(item)
    setEditRole(item.role)
    setEditActive(item.isActive)
    setError('')
  }

  async function saveEdit(event) {
    event.preventDefault()
    if (!editUser) return

    setSaving(true)
    setError('')
    try {
      const updated = await api.put(
        `/Users/${encodeURIComponent(editUser.keycloakUserId)}/access-control`,
        {
          role: editRole,
          isActive: editActive,
        },
      )
      setUsers((list) =>
        list.map((item) =>
          item.keycloakUserId === updated.keycloakUserId ? updated : item,
        ),
      )
      setEditUser(null)
    } catch (err) {
      setError(err.message)
    } finally {
      setSaving(false)
    }
  }

  // ---------- Разметка ----------

  return (
    <div className="dashboard users-access-page">
      <div className="page-head">
        <div>
          <h1>Пользователи и права доступа</h1>
          <p className="page-sub">
            Управление ролями пользователей и разграничение доступа к данным
          </p>
        </div>
      </div>

      {error && (
        <div className="form-error" role="alert">
          {error}
        </div>
      )}

      <div className="access-layout">
        {/* ---------- Таблица 1 (по макету): Список пользователей ---------- */}
        <section className="panel access-panel">
          <div className="panel-head access-panel-head">
            <div>
              <h2>Пользователи</h2>
              <p className="panel-sub">
                Список пользователей и их текущие роли
              </p>
            </div>
            <div className="access-search">
              <input
                type="search"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Поиск: ФИО, email, роль…"
                aria-label="Поиск пользователя"
              />
            </div>
          </div>

          {loading ? (
            <p className="page-loader">Загрузка…</p>
          ) : filteredUsers.length === 0 ? (
            <div className="access-empty">
              <IconUsers size={28} />
              <span>
                {users.length === 0
                  ? 'Пользователей не найдено.'
                  : 'Никто не соответствует поисковому запросу.'}
              </span>
            </div>
          ) : (
            <>
              <div className="table-wrap">
                <table className="access-users-table">
                  <thead>
                    <tr>
                      <th>ФИО</th>
                      <th>Роль</th>
                      <th>Статус</th>
                      <th className="access-actions-col">Действия</th>
                    </tr>
                  </thead>
                  <tbody>
                    {pagedUsers.map((item) => {
                      const selected = item.keycloakUserId === selectedId
                      const self = isCurrentUser(item)

                      return (
                        <tr
                          key={item.keycloakUserId}
                          className={selected ? 'is-selected' : ''}
                          onClick={() => setSelectedId(item.keycloakUserId)}
                        >
                          <td data-label="ФИО">
                            <button
                              type="button"
                              className="access-user-name"
                              onClick={() =>
                                setSelectedId(item.keycloakUserId)
                              }
                              title={item.email || item.userName || ''}
                            >
                              {fullName(item)}
                            </button>
                            {item.email && (
                              <div className="access-user-email">
                                {item.email}
                              </div>
                            )}
                          </td>
                          <td data-label="Роль">
                            <span
                              className={`role-badge role-badge--${item.role}`}
                            >
                              {ROLE_LABELS[item.role] ?? item.role}
                            </span>
                          </td>
                          <td data-label="Статус">
                            <span
                              className={
                                item.isActive
                                  ? 'status-badge is-final'
                                  : 'status-badge tone-5'
                              }
                            >
                              {item.isActive ? 'Активен' : 'Заблокирован'}
                            </span>
                          </td>
                          <td className="access-actions-col">
                            <button
                              type="button"
                              className="icon-action"
                              onClick={(event) => {
                                event.stopPropagation()
                                openEdit(item)
                              }}
                              disabled={self}
                              title={
                                self
                                  ? 'Текущий администратор'
                                  : 'Изменить роль и статус'
                              }
                              aria-label={
                                self
                                  ? 'Текущий администратор'
                                  : 'Изменить роль и статус'
                              }
                            >
                              <IconEdit size={16} />
                            </button>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>

              {/* Пейджер таблицы пользователей — как на странице «Вузы» */}
              {pageCount > 1 && (
                <nav className="pagination" aria-label="Страницы списка пользователей">
                  <span className="pagination-info">
                    Показано {shownFrom}–{shownTo} из {filteredUsers.length}
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

        {/* ---------- Таблица 2 (по макету): права выбранного пользователя ---------- */}
        <section className="panel access-panel">
          <div className="panel-head">
            <div>
              <h2>
                Права доступа
                {selectedUser && (
                  <span className="access-selected-user">
                    {' '}
                    — {fullName(selectedUser)}
                  </span>
                )}
              </h2>
              <p className="panel-sub">
                Права определяются ролью пользователя в Keycloak (п. 11 ТЗ)
              </p>
            </div>
          </div>

          {selectedUser ? (
            <div className="table-wrap">
              <table className="access-rights-table">
                <thead>
                  <tr>
                    <th>Раздел</th>
                    <th className="access-center">Просмотр</th>
                    <th className="access-center">Изменение</th>
                    <th className="access-center">Удаление</th>
                  </tr>
                </thead>
                <tbody>
                  {ACCESS_ROWS.map((row) => (
                    <tr key={row.section}>
                      <td data-label="Раздел">{row.section}</td>
                      <td className="access-center" data-label="Просмотр">
                        <AccessMark
                          allowed={row.view.includes(selectedUser.role)}
                        />
                      </td>
                      <td className="access-center" data-label="Изменение">
                        <AccessMark
                          allowed={row.edit.includes(selectedUser.role)}
                        />
                      </td>
                      <td className="access-center" data-label="Удаление">
                        <AccessMark
                          allowed={row.remove.includes(selectedUser.role)}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="access-empty">
              <IconUsers size={28} />
              <span>Выберите пользователя в таблице выше.</span>
            </div>
          )}
        </section>
      </div>

      {/* ---------- Модалка: смена роли и статуса ---------- */}
      {editUser && (
        <div
          className="modal-overlay"
          onMouseDown={() => !saving && setEditUser(null)}
        >
          <form
            className="modal access-user-modal"
            onSubmit={saveEdit}
            onMouseDown={(event) => event.stopPropagation()}
          >
            <h2>Настройка пользователя</h2>
            <p className="modal-user-title">{fullName(editUser)}</p>

            <label className="field">
              <span className="field-label">Роль</span>
              <select
                value={editRole}
                onChange={(event) => setEditRole(event.target.value)}
                disabled={saving}
              >
                <option value="user">Пользователь</option>
                <option value="manager">Руководитель</option>
                <option value="admin">Администратор</option>
              </select>
            </label>

            <label className="checkbox-field">
              <input
                type="checkbox"
                checked={editActive}
                onChange={(event) => setEditActive(event.target.checked)}
                disabled={saving}
              />
              Активная учётная запись
            </label>

            <div className="modal-actions">
              <button
                type="button"
                className="btn-ghost"
                onClick={() => setEditUser(null)}
                disabled={saving}
              >
                Отмена
              </button>
              <button type="submit" className="btn-primary" disabled={saving}>
                {saving ? 'Сохранение…' : 'Сохранить'}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  )
}