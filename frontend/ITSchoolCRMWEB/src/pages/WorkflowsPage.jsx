// Страница «Workflow» — раздел «Система», только admin (п. 11 ТЗ).
// Здесь администратор управляет самими workflow:
//   • создать новый workflow (например, B2C);
//   • переименовать workflow;
//   • добавить этап в любую позицию;
//   • переименовать существующий этап (запрет для не-админов
//     по уточнению заказчика 16.09.2026 12:16).
//
// Это ЕДИНСТВЕННОЕ место изменения схемы workflow: добавление этапов
// со страницы «Взаимодействия» сознательно убрано — оно применяется
// немедленно ко ВСЕМ взаимодействиям сценария, и такое действие
// безопасно выполнять только здесь, рядом с таблицей этапов.
//
// ВАЖНО (уточнение 12:13): workflow един для всех вузов, версионирование
// не допускается — любое изменение применяется немедленно ко всем
// взаимодействиям этого workflow. Поэтому на странице — предупреждение.
//
// АДАПТИВНОСТЬ: у каждого <td> таблицы .wf-table есть data-label с
// названием колонки — на мобильном (≤720px, см. base.css) таблица
// превращается в карточки. У колонки действий data-label нет.
//
// Контракт с бэкендом (ITSchoolCRM.API):
//   GET  /Workflows                          -> [{ id, name, description, isActive }]
//   GET  /Workflows/{id}                     -> { id, name, statuses[], transitions[] }
//   POST /Workflows                          { name, description }
//   PUT  /Workflows/{id}                     { name, description }
//   POST /Workflows/{id}/statuses            { name, description, insertAfterStatusId, isFinal }
//   PUT  /Workflows/{id}/statuses/{statusId} { name, description }

import { useEffect, useMemo, useState } from 'react'
import { api } from '../api/client'
import { IconEdit, IconPlus } from '../components/icons.jsx'

export default function WorkflowsPage() {
  // Список workflow и деталика выбранного
  const [workflows, setWorkflows] = useState([])
  const [selectedId, setSelectedId] = useState(null)
  const [detail, setDetail] = useState(null)
  const [loading, setLoading] = useState(true)
  const [pageError, setPageError] = useState('')
  const [pageNotice, setPageNotice] = useState('')

  // Модалки
  const [createOpen, setCreateOpen] = useState(false)
  const [renameWorkflowOpen, setRenameWorkflowOpen] = useState(false)
  const [addStatusOpen, setAddStatusOpen] = useState(false)
  const [renameStatusTarget, setRenameStatusTarget] = useState(null) // status object

  const [pending, setPending] = useState(false)
  const [formError, setFormError] = useState('')

  const [createForm, setCreateForm] = useState({
    name: '',
    description: '',
    firstStatusName: '',
  })
  const [renameWorkflowForm, setRenameWorkflowForm] = useState({
    name: '',
    description: '',
  })
  const [addStatusForm, setAddStatusForm] = useState({
    name: '',
    description: '',
    afterStatusId: '',
    isFinal: false,
  })
  const [renameStatusForm, setRenameStatusForm] = useState({
    name: '',
    description: '',
  })

  // ---------- Загрузка ----------

  function loadWorkflows(selectId = null) {
    return api
      .get('/Workflows')
      .then((list) => {
        setWorkflows(list ?? [])
        // Если передан id — выбираем его, иначе первый активный
        const nextId =
          selectId ??
          (list ?? []).find((w) => w.isActive !== false)?.id ??
          (list ?? [])[0]?.id ??
          null
        setSelectedId(nextId)
        return nextId
      })
      .catch((err) => setPageError(err.message))
  }

  function loadDetail(id) {
    if (!id) {
      setDetail(null)
      return Promise.resolve()
    }
    return api
      .get(`/Workflows/${id}`)
      .then((data) => setDetail(data))
      .catch((err) => setPageError(err.message))
  }

  useEffect(() => {
    setLoading(true)
    loadWorkflows()
      .then((id) => loadDetail(id))
      .finally(() => setLoading(false))
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    loadDetail(selectedId)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedId])

  const statuses = useMemo(
    () => [...(detail?.statuses ?? [])].sort((a, b) => a.sortOrder - b.sortOrder),
    [detail],
  )

  // ---------- Действия ----------

  function flashNotice(text) {
    setPageNotice(text)
    setPageError('')
    window.setTimeout(() => setPageNotice(''), 5000)
  }

  function openCreate() {
    setCreateForm({ name: '', description: '', firstStatusName: '' })
    setFormError('')
    setCreateOpen(true)
  }

  async function handleCreateSave(e) {
    e.preventDefault()
    if (!createForm.name.trim()) {
      setFormError('Введите название workflow.')
      return
    }
    if (!createForm.firstStatusName.trim()) {
      setFormError('Укажите название первого (стартового) этапа.')
      return
    }
    setPending(true)
    setFormError('')
    try {
      // 1. Создаём workflow
      const created = await api.post('/Workflows', {
        name: createForm.name.trim(),
        description: createForm.description.trim() || null,
      })
      // 2. Сразу добавляем стартовый этап (workflow без этапов нерабочий)
      await api.post(`/Workflows/${created.id}/statuses`, {
        name: createForm.firstStatusName.trim(),
        description: 'Стартовый этап',
        insertAfterStatusId: null,
        isFinal: false,
      })
      setCreateOpen(false)
      await loadWorkflows(created.id)
      await loadDetail(created.id)
      flashNotice(`Workflow «${created.name}» создан. Добавьте остальные этапы.`)
    } catch (err) {
      setFormError(err.message)
    } finally {
      setPending(false)
    }
  }

  function openRenameWorkflow() {
    if (!detail) return
    setRenameWorkflowForm({
      name: detail.name ?? '',
      description: detail.description ?? '',
    })
    setFormError('')
    setRenameWorkflowOpen(true)
  }

  async function handleRenameWorkflowSave(e) {
    e.preventDefault()
    if (!detail || !renameWorkflowForm.name.trim()) {
      setFormError('Введите название workflow.')
      return
    }
    setPending(true)
    setFormError('')
    try {
      await api.put(`/Workflows/${detail.id}`, {
        name: renameWorkflowForm.name.trim(),
        description: renameWorkflowForm.description.trim() || null,
      })
      setRenameWorkflowOpen(false)
      await loadWorkflows(detail.id)
      await loadDetail(detail.id)
      flashNotice('Workflow переименован.')
    } catch (err) {
      setFormError(err.message)
    } finally {
      setPending(false)
    }
  }

  function openAddStatus() {
    if (!statuses.length) return
    setAddStatusForm({
      name: '',
      description: '',
      afterStatusId: String(statuses[statuses.length - 1].id),
      isFinal: false,
    })
    setFormError('')
    setAddStatusOpen(true)
  }

  async function handleAddStatusSave(e) {
    e.preventDefault()
    if (!detail || !addStatusForm.name.trim()) {
      setFormError('Введите название этапа.')
      return
    }
    setPending(true)
    setFormError('')
    try {
      await api.post(`/Workflows/${detail.id}/statuses`, {
        name: addStatusForm.name.trim(),
        description: addStatusForm.description.trim() || null,
        insertAfterStatusId: addStatusForm.afterStatusId
          ? Number(addStatusForm.afterStatusId)
          : null,
        isFinal: addStatusForm.isFinal,
      })
      setAddStatusOpen(false)
      await loadDetail(detail.id)
      flashNotice('Этап добавлен — он сразу появился на картах всех взаимодействий.')
    } catch (err) {
      setFormError(err.message)
    } finally {
      setPending(false)
    }
  }

  function openRenameStatus(status) {
    setRenameStatusTarget(status)
    setRenameStatusForm({
      name: status.name ?? '',
      description: status.description ?? '',
    })
    setFormError('')
  }

  async function handleRenameStatusSave(e) {
    e.preventDefault()
    if (!detail || !renameStatusTarget || !renameStatusForm.name.trim()) {
      setFormError('Введите название этапа.')
      return
    }
    setPending(true)
    setFormError('')
    try {
      await api.put(`/Workflows/${detail.id}/statuses/${renameStatusTarget.id}`, {
        name: renameStatusForm.name.trim(),
        description: renameStatusForm.description.trim() || null,
      })
      setRenameStatusTarget(null)
      await loadDetail(detail.id)
      flashNotice('Этап переименован.')
    } catch (err) {
      setFormError(err.message)
    } finally {
      setPending(false)
    }
  }

  // ---------- Разметка ----------

  return (
    <div className="dashboard">
      {/* ---------- Шапка: контрастная пара кнопок ---------- */}
      <div className="page-head">
        <div>
          <h1>Workflow</h1>
          <p className="page-sub">
            Сценарии взаимодействия: B2B (вузы) и B2C (частные лица)
          </p>
        </div>
        <div className="page-actions">
          <button className="btn-ghost" onClick={openRenameWorkflow} disabled={!detail}>
            <IconEdit size={16} />
            Переименовать workflow
          </button>
          <button className="btn-primary" onClick={openCreate}>
            <IconPlus size={16} />
            Создать workflow
          </button>
        </div>
      </div>

      {pageError && <div className="form-error" role="alert">{pageError}</div>}
      {pageNotice && <div className="form-success" role="status">{pageNotice}</div>}

      <p className="doc-note">
        Workflow един для всех вузов: изменения применяются немедленно и ко всем
        взаимодействиям этого сценария. Версионирование не используется — перед
        изменением убедитесь, что сценарий согласован с командой.
      </p>

      {/* ---------- Переключатель workflow (B2B / B2C / …) ---------- */}
      <section className="panel">
        <div className="filters-grid filters-grid--uni">
          <label className="field filter-field">
            <span className="field-label">Сценарий</span>
            <select
              value={selectedId ?? ''}
              onChange={(e) => setSelectedId(Number(e.target.value))}
            >
              {workflows.map((w) => (
                <option key={w.id} value={String(w.id)}>
                  {w.name}
                  {w.isActive === false ? ' (неактивен)' : ''}
                </option>
              ))}
            </select>
          </label>
        </div>
      </section>

      {/* ---------- Этапы выбранного workflow: ровная таблица ---------- */}
      <section className="panel">
        <div className="panel-head">
          <div>
            <h2>{detail?.name ?? '…'}</h2>
            {detail?.description && (
              <p className="panel-sub">{detail.description}</p>
            )}
          </div>
          <div className="page-actions">
            <span className="status-badge tone-3">{statuses.length} этапов</span>
            <button
              className="btn-accent-soft"
              onClick={openAddStatus}
              disabled={!statuses.length}
            >
              <IconPlus size={16} />
              Добавить этап
            </button>
          </div>
        </div>

        {loading ? (
          <p className="page-loader">Загрузка…</p>
        ) : statuses.length === 0 ? (
          <p className="empty">
            В этом workflow пока нет этапов — нажмите «Добавить этап».
          </p>
        ) : (
          <div className="table-wrap">
            <table className="wf-table">
              <thead>
                <tr>
                  <th>Шаг</th>
                  <th>Название этапа</th>
                  <th>Описание</th>
                  <th>Тип</th>
                  <th className="col-actions">Действия</th>
                </tr>
              </thead>
              <tbody>
                {statuses.map((s) => (
                  <tr key={s.id}>
                    <td data-label="Шаг">
                      <span className="wf-step-num">{s.sortOrder}</span>
                    </td>
                    <td data-label="Название этапа">{s.name}</td>
                    <td className="wf-desc" data-label="Описание">
                      {s.description || '—'}
                    </td>
                    <td data-label="Тип">
                      {s.isInitial && (
                        <span className="status-badge tone-2">Стартовый</span>
                      )}
                      {s.isFinal && (
                        <span className="status-badge is-final">Финальный</span>
                      )}
                      {!s.isInitial && !s.isFinal && (
                        <span className="status-badge">Обычный</span>
                      )}
                    </td>
                    <td className="col-actions">
                      <button
                        type="button"
                        className="icon-btn row-action"
                        onClick={() => openRenameStatus(s)}
                        title="Переименовать этап"
                        aria-label="Переименовать этап"
                      >
                        <IconEdit size={16} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {/* ---------- Модалка: создать workflow ---------- */}
      {createOpen && (
        <div className="modal-overlay" onClick={() => setCreateOpen(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Создать workflow</h2>
            <p className="form-hint">
              Новый сценарий появится в списке доступных при загрузке JSON и
              интеграциях. Первый этап станет стартовым; остальные этапы
              добавляются здесь же (кнопка «Добавить этап»).
            </p>
            <form onSubmit={handleCreateSave} className="modal-form">
              <label className="field">
                <span className="field-label">Название</span>
                <input
                  type="text"
                  value={createForm.name}
                  onChange={(e) =>
                    setCreateForm((f) => ({ ...f, name: e.target.value }))
                  }
                  placeholder="Например: B2C Обучение частных лиц"
                  required
                />
              </label>
              <label className="field">
                <span className="field-label">Описание (необязательно)</span>
                <textarea
                  value={createForm.description}
                  onChange={(e) =>
                    setCreateForm((f) => ({ ...f, description: e.target.value }))
                  }
                  placeholder="Для каких взаимодействий используется этот сценарий"
                  rows={2}
                />
              </label>
              <label className="field">
                <span className="field-label">Первый (стартовый) этап</span>
                <input
                  type="text"
                  value={createForm.firstStatusName}
                  onChange={(e) =>
                    setCreateForm((f) => ({ ...f, firstStatusName: e.target.value }))
                  }
                  placeholder="Например: Заявка с сайта"
                  required
                />
              </label>
              {formError && <div className="form-error" role="alert">{formError}</div>}
              <div className="modal-actions">
                <button type="button" className="btn-ghost" onClick={() => setCreateOpen(false)}>
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={pending}>
                  {pending ? 'Создание…' : 'Создать'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ---------- Модалка: переименовать workflow ---------- */}
      {renameWorkflowOpen && (
        <div className="modal-overlay" onClick={() => setRenameWorkflowOpen(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Переименовать workflow</h2>
            <form onSubmit={handleRenameWorkflowSave} className="modal-form">
              <label className="field">
                <span className="field-label">Название</span>
                <input
                  type="text"
                  value={renameWorkflowForm.name}
                  onChange={(e) =>
                    setRenameWorkflowForm((f) => ({ ...f, name: e.target.value }))
                  }
                  required
                />
              </label>
              <label className="field">
                <span className="field-label">Описание</span>
                <textarea
                  value={renameWorkflowForm.description}
                  onChange={(e) =>
                    setRenameWorkflowForm((f) => ({ ...f, description: e.target.value }))
                  }
                  rows={2}
                />
              </label>
              {formError && <div className="form-error" role="alert">{formError}</div>}
              <div className="modal-actions">
                <button type="button" className="btn-ghost" onClick={() => setRenameWorkflowOpen(false)}>
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={pending}>
                  {pending ? 'Сохранение…' : 'Сохранить'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ---------- Модалка: добавить этап ---------- */}
      {addStatusOpen && (
        <div className="modal-overlay" onClick={() => setAddStatusOpen(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Добавить этап</h2>
            <p className="form-hint">
              Workflow: {detail?.name}. Новый этап появится на картах всех
              взаимодействий этого сценария.
            </p>
            <form onSubmit={handleAddStatusSave} className="modal-form">
              <label className="field">
                <span className="field-label">Название этапа</span>
                <input
                  type="text"
                  value={addStatusForm.name}
                  onChange={(e) =>
                    setAddStatusForm((f) => ({ ...f, name: e.target.value }))
                  }
                  placeholder="Например: Адаптация программы под вуз"
                  required
                />
              </label>
              <label className="field">
                <span className="field-label">Описание (необязательно)</span>
                <textarea
                  value={addStatusForm.description}
                  onChange={(e) =>
                    setAddStatusForm((f) => ({ ...f, description: e.target.value }))
                  }
                  placeholder="Что происходит на этом этапе"
                  rows={3}
                />
              </label>
              <label className="field">
                <span className="field-label">Вставить после этапа</span>
                <select
                  value={addStatusForm.afterStatusId}
                  onChange={(e) =>
                    setAddStatusForm((f) => ({ ...f, afterStatusId: e.target.value }))
                  }
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
                  checked={addStatusForm.isFinal}
                  onChange={(e) =>
                    setAddStatusForm((f) => ({ ...f, isFinal: e.target.checked }))
                  }
                />
                Финальный этап (завершает взаимодействие)
              </label>
              {formError && <div className="form-error" role="alert">{formError}</div>}
              <div className="modal-actions">
                <button type="button" className="btn-ghost" onClick={() => setAddStatusOpen(false)}>
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={pending}>
                  {pending ? 'Добавление…' : 'Добавить этап'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ---------- Модалка: переименовать этап ---------- */}
      {renameStatusTarget && (
        <div className="modal-overlay" onClick={() => setRenameStatusTarget(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Переименовать этап</h2>
            <p className="form-hint">
              Шаг {renameStatusTarget.sortOrder}. История переходов сохранится —
              изменится только отображаемое название.
            </p>
            <form onSubmit={handleRenameStatusSave} className="modal-form">
              <label className="field">
                <span className="field-label">Название этапа</span>
                <input
                  type="text"
                  value={renameStatusForm.name}
                  onChange={(e) =>
                    setRenameStatusForm((f) => ({ ...f, name: e.target.value }))
                  }
                  required
                />
              </label>
              <label className="field">
                <span className="field-label">Описание</span>
                <textarea
                  value={renameStatusForm.description}
                  onChange={(e) =>
                    setRenameStatusForm((f) => ({ ...f, description: e.target.value }))
                  }
                  rows={2}
                />
              </label>
              {formError && <div className="form-error" role="alert">{formError}</div>}
              <div className="modal-actions">
                <button type="button" className="btn-ghost" onClick={() => setRenameStatusTarget(null)}>
                  Отмена
                </button>
                <button type="submit" className="btn-primary" disabled={pending}>
                  {pending ? 'Сохранение…' : 'Сохранить'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}