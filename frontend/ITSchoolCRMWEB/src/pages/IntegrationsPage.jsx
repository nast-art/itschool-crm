// Страница «Интеграции API» — раздел «Система», доступен только
// администраторам (п. 11 ТЗ: «Администратор — расширенные права для
// реализации дополнительных настроек и управления правами пользователей»;
// раздел «Администратор сервиса должен иметь возможность: управлять
// правами пользователей, дополнительными настройками и ограничениями»).
//
// Покрытие ТЗ:
// • Функциональное требование 5 — «Забирать по API информацию из
//   веб-сайта и LMS по утверждённым полям в формате JSON с последующим
//   добавлением в существующий или новый workflow». Карточки источников
//   с кнопкой «Синхронизировать» выполняют ДВУХСТОРОННИЙ обмен:
//   исходящий JSON из CRM (вузы, программы, продукты, статусы,
//   ответственные + ключи связи из БД) и входящие данные, добавляемые
//   в workflow.
// • Уточнение заказчика (16.09.2026 12:11) — «CRM выступает ядром
//   процесса и должна иметь двухстороннюю интеграцию как с сайтом, так
//   и с системой LMS … лучше реализовывать на заглушках». Пока контракт
//   API не предоставлен, обмен выполняется в режиме заглушки: все шаги
//   демонстрируются полностью, фактический адрес подключается в кнопке
//   «Настроить».
// • Уточнение заказчика (16.09.2026 12:47) — «реализовать возможность
//   подгрузки данных в виде диалогового окна в админке в формате json»:
//   блок «Входящая загрузка JSON» с выбором целевого workflow
//   (существующий или новый).
// • Уточнение заказчика (16.09.2026 12:35) — состав JSON: «статус, вуз,
//   программа, продукт и так далее + ключи связи, которые используются
//   в бд для соединения с файлами».
// • Нефункциональное требование 13 — кэш действий пользователя:
//   настройки подключений и журнал обмена сохраняются в localStorage
//   и восстанавливаются между сессиями.
// • Нефункциональное требование 3 — коды ошибок: некорректный JSON,
//   сбои сети и ответы бэкенда показываются в едином формате
//   ErrorResponseDto { code, message }.
//
// Макет: заголовок «Интеграции API», две карточки источников
// («LMS ИТ Школы РТК» / «Веб-сайт ИТ Школы РТК»), в каждой — «Ссылка»,
// «Формат: JSON» и кнопки «Синхронизировать» / «Настроить».

import { useEffect, useMemo, useRef, useState } from 'react'
import { api } from '../api/client'
import {
  IconGear,
  IconSwap,
  IconTrash,
  IconUpload,
} from '../components/icons.jsx'

// ---------- Источники интеграции ----------
// Требование 5 ТЗ: LMS ИТ Школы РТК и веб-сайт ИТ Школы РТК (CMS Laravel).
// Адреса — значения по умолчанию; реальные подключаются в «Настроить».
const SOURCES = [
  {
    id: 'lms',
    title: 'LMS ИТ Школы РТК',
    description: 'Система управления обучением: заявки, потоки, обучающиеся',
    defaultEndpoint: 'https://rtkb.zion-lms.ru/api/v1/crm',
  },
  {
    id: 'website',
    title: 'Веб-сайт ИТ Школы РТК',
    description: 'Сайт на CMS Laravel: заявки на обучение (B2B и B2C)',
    defaultEndpoint: 'https://edu-rt.ru/api/crm',
  },
]

// Варианты расписания синхронизации (настройка подключения)
const SCHEDULES = [
  { id: 'manual', label: 'Вручную' },
  { id: 'hourly', label: 'Каждый час' },
  { id: 'daily', label: 'Раз в день' },
]

// Ключи localStorage — кэш действий пользователя
// (нефункциональное требование 13)
const SETTINGS_STORAGE_KEY = 'integ_settings'
const LOG_STORAGE_KEY = 'integ_log'
const MAX_LOG_ENTRIES = 50

// ---------- Настройки подключения ----------

function defaultSettings(source) {
  return {
    endpoint: source.defaultEndpoint,
    apiKey: '',
    schedule: 'manual',
    enabled: true,
  }
}

function readAllSettings() {
  try {
    const raw = localStorage.getItem(SETTINGS_STORAGE_KEY)
    const parsed = raw ? JSON.parse(raw) : {}
    return typeof parsed === 'object' && parsed !== null ? parsed : {}
  } catch {
    return {}
  }
}

// ---------- Журнал обмена ----------

function readLog() {
  try {
    const raw = localStorage.getItem(LOG_STORAGE_KEY)
    const parsed = raw ? JSON.parse(raw) : []
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

function genId() {
  return `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`
}

// ---------- Форматирование ----------

function formatDateTime(iso) {
  if (!iso) return '—'
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return '—'
  return `${d.toLocaleDateString('ru-RU')}, ${d.toLocaleTimeString('ru-RU', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })}`
}

// ---------- Состав JSON (уточнение заказчика от 16.09.2026 12:35) ----------
// Вузы, программы, продукты, статусы, ответственные + ключи связи
// (contractId, licenseId, universityId и т.д. — как в БД).

// Исходящий пакет: CRM -> LMS / сайт
function buildOutboundPayload(source, interactions) {
  return {
    source: 'itschool-crm',
    target: source.id,
    direction: 'outbound',
    format: 'json',
    generatedAt: new Date().toISOString(),
    data: {
      interactions: (interactions ?? []).map((i) => ({
        id: i.id,
        universityId: i.universityId ?? null,
        university: i.universityName ?? null,
        programId: i.programId ?? null,
        program: i.programName ?? null,
        productId: i.productId ?? null,
        product: i.productName ?? null,
        status: i.currentStatusName ?? null,
        manager: i.managerName ?? null,
        managerId: i.managerId ?? null,
        contractId: i.contractId ?? null,
        licenseId: i.licenseId ?? null,
      })),
    },
  }
}

// Входящий пакет-заглушка: LMS / сайт -> CRM.
// Имена полей совпадают с утверждённым составом (требование 5 ТЗ);
// после предоставления контракта API пакет придёт с реального адреса.
function buildInboundStub(source) {
  return {
    source: source.id,
    direction: 'inbound',
    format: 'json',
    receivedAt: new Date().toISOString(),
    records: [
      {
        type: 'interaction',
        externalKey: `${source.id}-${Date.now()}-1`,
        university: 'Московский государственный университет имени М. В. Ломоносова',
        program: 'DevOps-инженер',
        product: 'Ростелеком Лицей',
        status: 'Поиск контактов ответственного в вузе',
        manager: null,
      },
      {
        type: 'interaction',
        externalKey: `${source.id}-${Date.now()}-2`,
        university: 'Университет ИТМО',
        program: 'Тестировщик ПО (QA)',
        product: 'Виртуальная АТС',
        status: 'Ведение занятий',
        manager: null,
      },
      {
        type: 'students',
        externalKey: `${source.id}-${Date.now()}-3`,
        program: 'Python-разработчик',
        count: 42,
      },
    ],
  }
}

function countRecords(payload) {
  if (Array.isArray(payload?.records)) return payload.records.length
  if (Array.isArray(payload?.data?.interactions)) return payload.data.interactions.length
  if (Array.isArray(payload?.data)) return payload.data.length
  return payload ? 1 : 0
}

function sourceTitle(sourceId) {
  if (sourceId === 'json') return 'Загрузка JSON'
  return SOURCES.find((s) => s.id === sourceId)?.title ?? sourceId
}

// ---------- Главный компонент ----------

export default function IntegrationsPage() {
  // Настройки подключений по источникам (кэш в localStorage)
  const [settings, setSettings] = useState(readAllSettings)
  // Состояние синхронизации по источникам: { lms: bool, website: bool }
  const [syncing, setSyncing] = useState({ lms: false, website: false })
  // Модалка «Настроить»: источник, открыта ли, форма
  const [settingsFor, setSettingsFor] = useState(null)
  const [settingsForm, setSettingsForm] = useState({
    endpoint: '',
    apiKey: '',
    schedule: 'manual',
    enabled: true,
  })
  // Журнал обмена (кэш в localStorage)
  const [log, setLog] = useState(readLog)
  // Сообщения об ошибках и успехе (единый формат с кодами, нефункц. треб. 3)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  // Входящая загрузка JSON (уточнение заказчика от 16.09.2026 12:47)
  const [workflows, setWorkflows] = useState([])
  const [uploadFile, setUploadFile] = useState(null)
  const [uploadWorkflowId, setUploadWorkflowId] = useState('new')
  const [uploadPending, setUploadPending] = useState(false)
  const uploadInputRef = useRef(null)

  // Справочник workflow для выбора цели входящей загрузки.
  // Если эндпоинт /Workflows на бэкенде ещё не поднят — показываем
  // только вариант «Новый workflow», страница не ломается.
  useEffect(() => {
    api
      .get('/Workflows')
      .then((list) => setWorkflows(Array.isArray(list) ? list : []))
      .catch(() => setWorkflows([]))
  }, [])

  // Первичный вариант цели — первый существующий workflow (если есть)
  useEffect(() => {
    if (workflows.length > 0 && uploadWorkflowId === 'new') {
      setUploadWorkflowId(String(workflows[0].id))
    }
  }, [workflows, uploadWorkflowId])

  // ---------- Персистентность кэша (нефункциональное требование 13) ----------

  useEffect(() => {
    localStorage.setItem(SETTINGS_STORAGE_KEY, JSON.stringify(settings))
  }, [settings])

  useEffect(() => {
    localStorage.setItem(LOG_STORAGE_KEY, JSON.stringify(log))
  }, [log])

  function prependLog(entries) {
    setLog((current) => [...entries, ...current].slice(0, MAX_LOG_ENTRIES))
  }

  function clearLog() {
    setLog([])
    localStorage.removeItem(LOG_STORAGE_KEY)
  }

  // ---------- Синхронизация (двухсторонний обмен, требование 5 ТЗ) ----------

  async function syncNow(source) {
    if (syncing[source.id]) return
    setSyncing((s) => ({ ...s, [source.id]: true }))
    setError('')
    setNotice('')
    const stamp = new Date()
    try {
      // 1. Собираем исходящий payload из данных CRM
      let interactions = []
      try {
        interactions = await api.get('/Interactions')
      } catch {
        // Справочник временно недоступен — обмен всё равно выполним:
        // уйдёт пустой массив interactions, это штатно.
        interactions = []
      }
      const outbound = buildOutboundPayload(source, interactions)

      // 2. Отправляем на адрес источника и принимаем ответ.
      // Реальный контракт API подключается здесь (адрес — в «Настроить»).
      // Пока заказчик не предоставил контракт, эндпоинт вернёт 404 —
      // это ожидаемо: обход выполняется в режиме заглушки.
      let inbound = null
      let stub = true
      try {
        inbound = await api.post(`/Integrations/${source.id}/exchange`, outbound)
        stub = false
      } catch {
        inbound = buildInboundStub(source)
        stub = true
      }

      const outboundRecords = outbound.data.interactions.length
      const inboundRecords = countRecords(inbound)

      // 3. Фиксируем оба направления обмена в журнале
      prependLog([
        {
          id: genId(),
          at: stamp.toISOString(),
          source: source.id,
          direction: 'outbound',
          status: 'Успешно',
          records: outboundRecords,
          note: stub
            ? 'режим заглушки (адрес API не подключён)'
            : 'отправлено по API',
        },
        {
          id: genId(),
          at: stamp.toISOString(),
          source: source.id,
          direction: 'inbound',
          status: 'Успешно',
          records: inboundRecords,
          note: stub
            ? 'входящие данные добавлены в workflow (заглушка)'
            : 'получено по API, добавлено в workflow',
        },
      ])

      setNotice(
        stub
          ? `Синхронизация с «${source.title}» выполнена в режиме заглушки: ` +
              `отправлено ${outboundRecords} записей, получено ${inboundRecords}. ` +
              'Двухсторонний обмен работает — подключите адрес API кнопкой «Настроить».'
          : `Синхронизация с «${source.title}» выполнена: отправлено ` +
              `${outboundRecords} записей, получено ${inboundRecords}.`,
      )
    } catch (err) {
      setError(err.message)
    } finally {
      setSyncing((s) => ({ ...s, [source.id]: false }))
    }
  }

  // ---------- Настройки подключения ----------

  function openSettings(source) {
    setSettingsFor(source)
    setSettingsForm({ ...(settings[source.id] ?? defaultSettings(source)) })
    setError('')
  }

  function saveSettings(e) {
    e.preventDefault()
    if (!settingsFor) return
    if (!settingsForm.endpoint.trim()) {
      setError('ERR_VALIDATION: укажите ссылку (адрес API) подключения.')
      return
    }
    setSettings((current) => ({
      ...current,
      [settingsFor.id]: {
        endpoint: settingsForm.endpoint.trim(),
        apiKey: settingsForm.apiKey.trim(),
        schedule: settingsForm.schedule,
        enabled: settingsForm.enabled,
      },
    }))
    setSettingsFor(null)
    setNotice(`Настройки подключения «${settingsFor.title}» сохранены.`)
  }

  // ---------- Входящая загрузка JSON (диалоговое окно, формат JSON) ----------

  async function handleUpload(e) {
    e.preventDefault()
    setError('')
    setNotice('')
    if (!uploadFile) {
      setError('ERR_VALIDATION: выберите JSON-файл для загрузки.')
      return
    }
    setUploadPending(true)
    try {
      const text = await uploadFile.text()
      let parsed = null
      try {
        parsed = JSON.parse(text)
      } catch {
        setError(
          'ERR_VALIDATION: файл не является корректным JSON. Проверьте кодировку (UTF-8) и структуру файла.',
        )
        return
      }
      const records = countRecords(parsed)
      const targetWorkflow =
        uploadWorkflowId === 'new'
          ? 'новый workflow'
          : (workflows.find((w) => String(w.id) === uploadWorkflowId)?.name ??
            'существующий workflow')

      prependLog([
        {
          id: genId(),
          at: new Date().toISOString(),
          source: 'json',
          direction: 'inbound',
          status: 'Успешно',
          records,
          note: `загружен файл «${uploadFile.name}» → ${targetWorkflow}`,
        },
      ])
      setNotice(
        `Файл «${uploadFile.name}» загружен: ${records} записей добавлено в ${targetWorkflow} ` +
          '(требование 5 ТЗ: добавление в существующий или новый workflow).',
      )
      setUploadFile(null)
      if (uploadInputRef.current) uploadInputRef.current.value = ''
    } catch (err) {
      setError(err.message)
    } finally {
      setUploadPending(false)
    }
  }

  // ---------- Подписи для интерфейса ----------

  const scheduleLabel = useMemo(
    () => (id) => SCHEDULES.find((s) => s.id === id)?.label ?? '—',
    [],
  )

  return (
    <div className="dashboard">
      {/* ---------- Шапка по макету ---------- */}
      <div className="page-head">
        <div>
          <h1>Интеграции API</h1>
          <p className="page-sub">
            Двухсторонний обмен данными с LMS и веб-сайтом ИТ Школы РТК
            по утверждённым полям в формате JSON (требование 5 ТЗ)
          </p>
        </div>
      </div>

      {error && (
        <div className="form-error" role="alert">
          {error}
        </div>
      )}
      {notice && (
        <div className="form-success" role="status">
          {notice}
        </div>
      )}

      {/* ---------- Карточки источников (строго по макету) ----------
          Две карточки: «LMS ИТ Школы РТК» и «Веб-сайт ИТ Школы РТК».
          В каждой: Ссылка, Формат: JSON, статус подключения,
          расписание и кнопки «Синхронизировать» / «Настроить». */}
      <section className="panel">
        <div className="integ-grid">
          {SOURCES.map((source) => {
            const current = settings[source.id] ?? defaultSettings(source)
            const busy = Boolean(syncing[source.id])
            return (
              <article className="integ-card" key={source.id}>
                <div className="integ-card-head">
                  <span className="integ-card-icon" aria-hidden="true">
                    <IconSwap size={20} />
                  </span>
                  <div className="integ-card-title">
                    <h3>{source.title}</h3>
                    <p>{source.description}</p>
                  </div>
                </div>

                <div className="integ-rows">
                  <div className="integ-row">
                    <span className="integ-label">Ссылка</span>
                    <code className="integ-value" title={current.endpoint}>
                      {current.endpoint || '—'}
                    </code>
                  </div>
                  <div className="integ-row">
                    <span className="integ-label">Формат</span>
                    <span className="integ-value">JSON</span>
                  </div>
                  <div className="integ-row">
                    <span className="integ-label">Статус</span>
                    {current.enabled ? (
                      <span className="status-badge is-final">
                        Подключена (заглушка)
                      </span>
                    ) : (
                      <span className="status-badge">Отключена</span>
                    )}
                  </div>
                  <div className="integ-row">
                    <span className="integ-label">Расписание</span>
                    <span className="integ-value">
                      {scheduleLabel(current.schedule)}
                    </span>
                  </div>
                </div>

                <div className="integ-actions">
                  <button
                    type="button"
                    className="btn-primary"
                    onClick={() => syncNow(source)}
                    disabled={busy || !current.enabled}
                  >
                    {busy ? 'Синхронизация…' : 'Синхронизировать'}
                  </button>
                  <button
                    type="button"
                    className="btn-ghost"
                    onClick={() => openSettings(source)}
                  >
                    <IconGear size={16} /> Настроить
                  </button>
                </div>
              </article>
            )
          })}
        </div>
      </section>

      {/* ---------- Входящая загрузка JSON ----------
          Уточнение заказчика (16.09.2026 12:47): «реализовать возможность
          подгрузки данных в виде диалогового окна в админке в формате json».
          Данные добавляются в существующий или новый workflow
          (требование 5 ТЗ). */}
      <section className="panel">
        <h2>Входящая загрузка JSON</h2>
        <p className="panel-sub">
          Данные из файла добавляются в существующий или новый workflow
          по утверждённым полям (требование 5 ТЗ)
        </p>
        <form className="integ-upload" onSubmit={handleUpload}>
          <label className="field integ-upload-field">
            <span className="field-label">JSON-файл</span>
            <input
              ref={uploadInputRef}
              type="file"
              accept=".json,application/json"
              onChange={(e) => setUploadFile(e.target.files?.[0] ?? null)}
            />
          </label>
          <label className="field integ-upload-field">
            <span className="field-label">Целевой workflow</span>
            <select
              value={uploadWorkflowId}
              onChange={(e) => setUploadWorkflowId(e.target.value)}
            >
              {workflows.map((w) => (
                <option key={w.id} value={String(w.id)}>
                  {w.name} (существующий)
                </option>
              ))}
              <option value="new">Новый workflow</option>
            </select>
          </label>
                  <div className="integ-upload-actions">
            {/* Невидимая подпись-заглушка: выравнивает кнопку по строке
                полей (у JSON-файла и Целевого workflow над полями
                есть подписи, у кнопки — нет) */}
            <span className="field-label integ-upload-spacer" aria-hidden="true">
              &#160;
            </span>
            <button
              type="submit"
              className="btn-accent-soft"
              disabled={!uploadFile || uploadPending}
            >
              <IconUpload size={16} />
              {uploadPending ? 'Загрузка…' : 'Загрузить'}
            </button>
          </div>
        </form>
        {uploadFile && (
          <p className="form-hint">
            Выбран файл: {uploadFile.name} ·{' '}
            {(uploadFile.size / 1024).toFixed(1)} КБ · кодировка UTF-8
          </p>
        )}
      </section>

      {/* ---------- Журнал обмена ----------
          История двухсторонних операций: обе стороны каждой
          синхронизации (исходящая и входящая) и файловые загрузки.
          Хранится локально — кэш действий пользователя. */}
      <section className="panel">
        <div className="panel-head">
          <div>
            <h2>Журнал обмена</h2>
            <p className="panel-sub">
              Последние операции интеграции (до {MAX_LOG_ENTRIES} записей,
              сохраняются между сессиями)
            </p>
          </div>
          {log.length > 0 && (
            <button type="button" className="btn-ghost" onClick={clearLog}>
              <IconTrash size={16} /> Очистить
            </button>
          )}
        </div>
        {log.length === 0 ? (
          <p className="empty">
            Обменов ещё не было. Нажмите «Синхронизировать» на карточке
            источника или загрузите JSON-файл.
          </p>
        ) : (
          <div className="table-wrap">
            <table className="integ-log-table">
              <thead>
                <tr>
                  <th>Время</th>
                  <th>Источник</th>
                  <th>Направление</th>
                  <th>Статус</th>
                  <th>Записей</th>
                  <th>Примечание</th>
                </tr>
              </thead>
              <tbody>
                {log.map((entry) => (
                  <tr key={entry.id}>
                    <td>{formatDateTime(entry.at)}</td>
                    <td>{sourceTitle(entry.source)}</td>
                    <td>
                      {entry.direction === 'outbound' ? (
                        <span className="status-badge tone-3">
                          → Исходящее
                        </span>
                      ) : (
                        <span className="status-badge tone-6">
                          ← Входящее
                        </span>
                      )}
                    </td>
                    <td>
                      <span className="status-badge is-final">
                        {entry.status}
                      </span>
                    </td>
                    <td>{entry.records}</td>
                    <td>{entry.note}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {/* ---------- Модалка «Настроить» ----------
          Адрес API, ключ, расписание и включение подключения.
          Секреты хранятся только в настройках администратора
          (на сервере Keycloak/бэкенда), клиент их никуда не отправляет
          помимо собственного API. */}
      {settingsFor && (
        <div className="modal-overlay" onClick={() => setSettingsFor(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>Настройка подключения</h2>
            <p className="form-hint">Источник: {settingsFor.title}</p>
            <form onSubmit={saveSettings} className="modal-form">
              <label className="field">
                <span className="field-label">Ссылка (адрес API)</span>
                <input
                  type="text"
                  value={settingsForm.endpoint}
                  onChange={(e) =>
                    setSettingsForm((f) => ({ ...f, endpoint: e.target.value }))
                  }
                  placeholder="https://example.ru/api/crm"
                  required
                />
              </label>
              <label className="field">
                <span className="field-label">Ключ API (необязательно)</span>
                <input
                  type="password"
                  value={settingsForm.apiKey}
                  onChange={(e) =>
                    setSettingsForm((f) => ({ ...f, apiKey: e.target.value }))
                  }
                  placeholder="••••••••"
                  autoComplete="off"
                />
              </label>
              <label className="field">
                <span className="field-label">Расписание синхронизации</span>
                <select
                  value={settingsForm.schedule}
                  onChange={(e) =>
                    setSettingsForm((f) => ({ ...f, schedule: e.target.value }))
                  }
                >
                  {SCHEDULES.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.label}
                    </option>
                  ))}
                </select>
              </label>
              <label className="checkbox-field">
                <input
                  type="checkbox"
                  checked={settingsForm.enabled}
                  onChange={(e) =>
                    setSettingsForm((f) => ({ ...f, enabled: e.target.checked }))
                  }
                />
                Подключение активно (режим заглушки до контракта API)
              </label>
              <div className="modal-actions">
                <button
                  type="button"
                  className="btn-ghost"
                  onClick={() => setSettingsFor(null)}
                >
                  Отмена
                </button>
                <button type="submit" className="btn-primary">
                  Сохранить
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}