// Страница «ИТ-направления» — каталог образовательных программ
// по ИТ-направлениям (требование 1 ТЗ: каталоги в БД).
// Состав по макету:
//   1. Шапка: «ИТ-программы и продукты» + подзаголовок.
//   2. Сетка карточек направлений: название, описание, статистика
//      («N вузов» — вузы с взаимодействиями по направлению,
//      «N модулей» — программы направления), кнопка «Открыть workflow»
//      — переход на страницу «Взаимодействия» С ФИЛЬТРОМ по этому
//      направлению (?direction={id}).
//   3. Карточка «Добавить программу» (+ Новое ИТ-направление или
//      продукт) — только руководитель и администратор.
//   4. Ранжирование по востребованности (уточнение заказчика:
//      «иметь возможность изменения приоритетов»): привилегированные
//      роли могут менять приоритет направления (▲▼), сортировка
//      карточек — по приоритету, затем по числу вузов.
//
// РОЛЕВАЯ МОДЕЛЬ (п. 11 ТЗ):
//   user    — только просмотр и переход на workflow;
//   manager — + добавление программ/направлений, изменение приоритетов;
//   admin   — всё вышеперечисленное.
//
// Контракт с бэкендом (ITSchoolCRM.API):
//   GET  /Directions        — справочник ИТ-направлений
//   GET  /Programs          — справочник ИТ-программ (directionId)
//   GET  /Interactions      — взаимодействия (для статистики по вузам)
//   POST /Directions        — новое направление (manager/admin)
//   POST /Programs          — новая программа (manager/admin)
//   PUT  /Directions/{id}   — изменение приоритета (manager/admin)
import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../App.jsx'
import {
  IconArrowRight,
  IconBuilding,
  IconLayers,
  IconPlus,
} from '../components/icons.jsx'

// ---------- Главный компонент ----------

export default function DirectionsPage() {
  const navigate = useNavigate()
  const { user } = useAuth()

  // Ролевая модель (п. 11 ТЗ) — управляет видимостью
  // карточки «Добавить программу» и приоритетов
  const roles = user?.roles ?? []
  const isAdmin = roles.includes('admin')
  const isManager = roles.includes('manager')
  const canManage = isAdmin || isManager

  // Справочники и статистика
  const [directions, setDirections] = useState([])
  const [programs, setPrograms] = useState([])
  const [interactions, setInteractions] = useState([])
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState('')

  // Модалка «Добавить программу»
  const [addOpen, setAddOpen] = useState(false)
  const [addPending, setAddPending] = useState(false)
  const [addError, setAddError] = useState('')
  const [addForm, setAddForm] = useState({
    type: 'program', // 'program' | 'direction'
    name: '',
    directionId: '',
    description: '',
  })

  // Приоритеты: локальный черновик, пока не сохранён
  const [priorityDraft, setPriorityDraft] = useState({})
  const [prioritySavingId, setPrioritySavingId] = useState(null)
  const [priorityError, setPriorityError] = useState('')

  // ---------- Загрузка данных (один раз) ----------

  useEffect(() => {
    setLoading(true)
    Promise.all([
      api.get('/Directions'),
      api.get('/Programs'),
      api.get('/Interactions'),
    ])
      .then(([dirs, progs, list]) => {
        setDirections(dirs ?? [])
        setPrograms(progs ?? [])
        setInteractions(list ?? [])
      })
      .catch((err) => setPageError(err.message))
      .finally(() => setLoading(false))
  }, [])

  // ---------- Статистика по направлениям ----------
  // «N вузов» — уникальные вузы с взаимодействиями по программам
  // направления; «N модулей» — количество программ направления.

  const programDirectionMap = useMemo(() => {
    const map = {}
    programs.forEach((p) => {
      map[p.id] = p.directionId
    })
    return map
  }, [programs])

  const statsByDirection = useMemo(() => {
    const map = {}
    interactions.forEach((i) => {
      const directionId = programDirectionMap[i.programId]
      if (directionId == null) return
      if (!map[directionId]) {
        map[directionId] = { universities: new Set(), interactions: 0 }
      }
      if (i.universityId != null) {
        map[directionId].universities.add(i.universityId)
      }
      map[directionId].interactions += 1
    })
    return map
  }, [interactions, programDirectionMap])

  const programsCountByDirection = useMemo(() => {
    const map = {}
    programs.forEach((p) => {
      if (p.directionId != null) {
        map[p.directionId] = (map[p.directionId] ?? 0) + 1
      }
    })
    return map
  }, [programs])

  // ---------- Ранжирование ----------
  // Сортировка: по приоритету (меньше = выше), без приоритета —
  // по востребованности (числу вузов). Так реализован «простой путь»
  // ранжирования программ по востребованности с ручной корректировкой
  // приоритетов (уточнение заказчика).

  function priorityOf(direction) {
    if (direction.id in priorityDraft) {
      return priorityDraft[direction.id]
    }
    return direction.priority ?? null
  }

  const sortedDirections = useMemo(
    () =>
      [...directions].sort((a, b) => {
        const pa = priorityOf(a)
        const pb = priorityOf(b)
        if (pa != null && pb != null && pa !== pb) return pa - pb
        if (pa != null && pb == null) return -1
        if (pa == null && pb != null) return 1
        const ua = statsByDirection[a.id]?.universities.size ?? 0
        const ub = statsByDirection[b.id]?.universities.size ?? 0
        if (ua !== ub) return ub - ua
        return (a.name ?? '').localeCompare(b.name ?? '', 'ru')
      }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [directions, statsByDirection, priorityDraft],
  )

  // ---------- Изменение приоритета (manager/admin) ----------

  function changePriorityDraft(direction, delta) {
    const current = priorityOf(direction) ?? 0
    setPriorityDraft((d) => ({ ...d, [direction.id]: current + delta }))
    setPriorityError('')
  }

  async function savePriority(direction) {
    if (!(direction.id in priorityDraft)) return
    setPrioritySavingId(direction.id)
    setPriorityError('')
    try {
      await api.put(`/Directions/${direction.id}`, {
        name: direction.name,
        description: direction.description ?? null,
        priority: priorityDraft[direction.id],
      })
      setDirections((list) =>
        list.map((d) =>
          d.id === direction.id ? { ...d, priority: priorityDraft[direction.id] } : d,
        ),
      )
      setPriorityDraft((d) => {
        const next = { ...d }
        delete next[direction.id]
        return next
      })
    } catch (err) {
      setPriorityError(err.message)
    } finally {
      setPrioritySavingId(null)
    }
  }

  // ---------- Добавление программы / направления ----------

  function openAdd() {
    setAddForm({ type: 'program', name: '', directionId: '', description: '' })
    setAddError('')
    setAddOpen(true)
  }

  async function handleAddSave(e) {
    e.preventDefault()
    if (!addForm.name.trim()) {
      setAddError('Введите название.')
      return
    }
    if (addForm.type === 'program' && !addForm.directionId) {
      setAddError('Выберите ИТ-направление для программы.')
      return
    }
    setAddPending(true)
    setAddError('')
    try {
      if (addForm.type === 'direction') {
        // Новое ИТ-направление
        await api.post('/Directions', {
          name: addForm.name.trim(),
          description: addForm.description.trim() || null,
        })
      } else {
        // Новая ИТ-программа (модуль) в рамках направления
        await api.post('/Programs', {
          name: addForm.name.trim(),
          directionId: Number(addForm.directionId),
          description: addForm.description.trim() || null,
        })
      }
      setAddOpen(false)
      // Перечитываем справочники
      const [dirs, progs] = await Promise.all([
        api.get('/Directions'),
        api.get('/Programs'),
      ])
      setDirections(dirs ?? [])
      setPrograms(progs ?? [])
    } catch (err) {
      setAddError(err.message)
    } finally {
      setAddPending(false)
    }
  }

  // ---------- Разметка ----------

  return (
    <div className="dashboard">
      <div className="page-head">
        <div>
          <h1>ИТ-направления</h1>
          <p className="page-sub">Каталог образовательных программ по ИТ-направлениям</p>
        </div>
      </div>

      {pageError && <div className="form-error" role="alert">{pageError}</div>}
      {priorityError && <div className="form-error" role="alert">{priorityError}</div>}

      {loading ? (
        <p className="page-loader">Загрузка…</p>
      ) : (
        <div className="directions-grid">
          {/* ---------- Карточки направлений ---------- */}
          {sortedDirections.map((d) => {
            const stats = statsByDirection[d.id]
            const universitiesCount = stats?.universities.size ?? 0
            const modulesCount = programsCountByDirection[d.id] ?? 0
            const hasDraft = d.id in priorityDraft

            return (
              <article key={d.id} className="direction-card">
                <div className="dir-card-head">
                  <h3>{d.name}</h3>
                  {canManage && (
                    // Ранжирование по востребованности: ручная
                    // корректировка приоритета (manager/admin)
                    <div className="priority-control" title="Приоритет (меньше — выше в списке)">
                      <button
                        type="button"
                        className="priority-btn"
                        onClick={() => changePriorityDraft(d, -1)}
                        disabled={prioritySavingId === d.id}
                        aria-label="Повысить приоритет"
                      >
                        ▲
                      </button>
                      <span className="priority-value">
                        {priorityOf(d) ?? '—'}
                      </span>
                      <button
                        type="button"
                        className="priority-btn"
                        onClick={() => changePriorityDraft(d, 1)}
                        disabled={prioritySavingId === d.id}
                        aria-label="Понизить приоритет"
                      >
                        ▼
                      </button>
                      {hasDraft && (
                        <button
                          type="button"
                          className="priority-save"
                          onClick={() => savePriority(d)}
                          disabled={prioritySavingId === d.id}
                        >
                          {prioritySavingId === d.id ? '…' : '✓'}
                        </button>
                      )}
                    </div>
                  )}
                </div>

                {d.description && <p className="dir-desc">{d.description}</p>}

                <div className="dir-stats">
                  <span className="dir-stat" title="Вузы с взаимодействиями по направлению">
                    <IconBuilding size={16} />
                    {universitiesCount} вузов
                  </span>
                  <span className="dir-stat" title="Программы (модули) направления">
                    <IconLayers size={16} />
                    {modulesCount} модулей
                  </span>
                </div>

                {/* Кнопка перехода на страницу взаимодействия (workflow)
                    с фильтром по этому направлению — по макету */}
                <div className="dir-card-actions">
                  <button
                    type="button"
                    className="btn-ghost"
                    onClick={() => navigate(`/interactions?direction=${d.id}`)}
                  >
                    Открыть workflow
                    <IconArrowRight size={16} />
                  </button>
                </div>
              </article>
            )
          })}

          {/* ---------- Карточка добавления (manager/admin) ---------- */}
          {canManage && (
            <button type="button" className="direction-card dir-add-card" onClick={openAdd}>
              <span className="dir-add-plus">
                <IconPlus size={22} />
              </span>
              <strong>Добавить программу</strong>
              <span className="dir-add-hint">Новое ИТ-направление или продукт</span>
            </button>
          )}
        </div>
      )}

      {/* ---------- Модалка: добавление программы / направления ---------- */}
      {addOpen && (
        <div className="modal-overlay" onClick={() => setAddOpen(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Добавить программу</h2>
            <p className="form-hint">
              Создайте новое ИТ-направление или программу (модуль) в рамках существующего.
            </p>
            <form onSubmit={handleAddSave} className="modal-form">
              <label className="field">
                <span className="field-label">Тип</span>
                <select
                  value={addForm.type}
                  onChange={(e) => setAddForm((f) => ({ ...f, type: e.target.value }))}
                >
                  <option value="program">ИТ-программа (модуль)</option>
                  <option value="direction">ИТ-направление</option>
                </select>
              </label>
              <label className="field">
                <span className="field-label">Название</span>
                <input
                  type="text"
                  value={addForm.name}
                  onChange={(e) => setAddForm((f) => ({ ...f, name: e.target.value }))}
                  placeholder={
                    addForm.type === 'direction' ? 'Например: DevOps' : 'Например: DevOps-инженер'
                  }
                  required
                />
              </label>
              {addForm.type === 'program' && (
                <label className="field">
                  <span className="field-label">ИТ-направление</span>
                  <select
                    value={addForm.directionId}
                    onChange={(e) => setAddForm((f) => ({ ...f, directionId: e.target.value }))}
                  >
                    <option value="">Выберите направление</option>
                    {directions.map((d) => (
                      <option key={d.id} value={String(d.id)}>
                        {d.name}
                      </option>
                    ))}
                  </select>
                </label>
              )}
              <label className="field">
                <span className="field-label">Описание (необязательно)</span>
                <textarea
                  value={addForm.description}
                  onChange={(e) => setAddForm((f) => ({ ...f, description: e.target.value }))}
                  placeholder="Краткое описание"
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
    </div>
  )
}