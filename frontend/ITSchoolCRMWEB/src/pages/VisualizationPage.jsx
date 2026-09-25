// Страница «Визуализация» — статистика в формате диаграмм и графиков
// строго по макету:
//   [ Визуализация данных                                    [Экспорт ▾] ]
//   [ Заявки на обучения по месяцам (график) ] [ Количество обучающихся
//                                              по направлениям (диаграмма) ]
//   [ Количество параллельных потоков (график) — на всю ширину          ]
//
// Покрытие ТЗ:
//  • Функциональное требование 1 — фильтрация вывода данных за выбранный
//    период по выбранным вузам, ИТ-направлениям, ИТ-продуктам и
//    ответственным — фильтр «Ответственный» доступен ВСЕМ ролям, в ТЗ
//    он не делится по ролям (см. ролевую модель ниже).
//  • Функциональное требование 1 — визуализация статистики в форматах
//    png, pdf: при нажатии «Экспорт» появляются 2 варианта (PNG, PDF),
//    как указано стрелкой на макете. Выгружаются ВСЕ графики или
//    выбранный ОДИН (по усмотрению пользователя).
//  • Уточнение заказчика (16.09.2026 12:42) — интерактивные графики с
//    возможностью изменения параметров/фильтров ДО выгрузки отчёта.
//  • Нефункциональное требование 13 — кэш действий пользователя:
//    период и фильтры сохраняются в localStorage и восстанавливаются.
//  • Ролевая модель (п. 11 ТЗ): фильтрация и экспорт PNG/PDF доступны
//    ВСЕМ ролям, включая фильтр по ответственным — в ТЗ он не делится
//    по ролям: «пользователь … формирует отчёты по выбранным вузам,
//    ИТ-направлениям, ИТ-продуктам, ответственным за них».
//    Привилегия руководителя по ТЗ — ИЗМЕНЯТЬ ответственных за вузы
//    (назначать, менять, удалять), а не фильтровать по ним.
//    admin — расширенные права: дополнительно экспорт результирующего
//    JSON (п. 4 «Требований к решению»).
//  • Разграничение по данным (пользователь видит только свои вузы)
//    выполняет бэкенд через university_managers — фронт дублирует
//    ограничения не перекрывая их.
//
// Контракт с бэкендом (ITSchoolCRM.API):
//   GET /Universities — справочник вузов
//   GET /Directions   — справочник ИТ-направлений
//   GET /Programs     — ИТ-программы (directionId для клиентской фильтрации)
//   GET /Products     — ИТ-продукты
//   GET /Users        — пользователи (фильтр «Ответственный», все роли)
//   GET /Interactions — взаимодействия (уже отфильтрованы по доступу)
//   POST /Reports/statistics — агрегаты по фильтру (byDirection и т.д.)
//
// Зависимости экспорта:
//   PNG — не требует пакетов (сериализация SVG + canvas);
//   PDF — опционально: npm i jspdf (динамический импорт).

import { useEffect, useMemo, useRef, useState } from 'react'
import { api } from '../api/client'
import { getStatistics } from '../api/reports.js'
import { useAuth } from '../App.jsx'
import { IconChevronDown, IconDownload, IconReport } from '../components/icons.jsx'
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
// Экспорт графиков: НЕ требует пакетов. PNG строится из SVG графика
// встроенными средствами браузера (XMLSerializer + canvas) — страница
// собирается и работает всегда. PDF собирается через jspdf, который
// подключается ДИНАМИЧЕСКИ (await import): если пакет не установлен,
// страница работает, а кнопка PDF покажет понятную ошибку с командой
// установки. Установка опциональна: npm i jspdf

// ---------- Константы ----------

// Периоды обзора — календарные: неделя с понедельника, месяц с 1-го числа,
// квартал с 1-го дня текущего квартала, год с 1 января, «всё время».
const PERIODS = [
  { id: 'all', label: 'Все время' },
  { id: 'week', label: 'Неделя' },
  { id: 'month', label: 'Месяц' },
  { id: 'quarter', label: 'Квартал' },
  { id: 'year', label: 'Год' },
]

// Начало календарного периода по его id ('all' -> null = без ограничения)
function periodStart(id) {
  if (id === 'all') return null
  const now = new Date()

  if (id === 'week') {
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
  return null
}

const MONTHS = [
  'Янв', 'Фев', 'Мар', 'Апр', 'Май', 'Июн',
  'Июл', 'Авг', 'Сен', 'Окт', 'Ноя', 'Дек',
]

// Палитра диаграммы — статусные цвета ДС Ростелеком.
// 12 оттенков: направлений в справочнике 10, циклический перебор
// гарантирует корректные цвета при любом их числе.
const PIE_COLORS = [
  '#8b31ff', '#5b7cfa', '#2aa7f0', '#26b8a8', '#d73bd0', '#f0447e',
  '#ff4d14', '#22bf67', '#ffae1a', '#2e7cf6', '#f9392d', '#c77800',
]

// Ключи localStorage — кэш действий пользователя (нефункц. требование 13)
const LS_PERIOD = 'viz_period'
const LS_UNIVERSITIES = 'viz_filter_universities'
const LS_DIRECTIONS = 'viz_filter_directions'
const LS_PRODUCTS = 'viz_filter_products'
const LS_MANAGERS = 'viz_filter_managers'

// Восстановление сохранённых фильтров из localStorage
function readStoredIds(key) {
  try {
    const raw = localStorage.getItem(key)
    const parsed = raw ? JSON.parse(raw) : []
    return Array.isArray(parsed) ? parsed.filter((x) => Number.isInteger(x)) : []
  } catch {
    return []
  }
}

// Усечение длинных подписей многоточием — используется только
// в легенде экспорта (там колонки узкие). На экране подписи осей
// НЕ усечаются: график потоков горизонтальный и показывает
// названия направлений полностью.
function truncateLabel(value, max = 20) {
  const s = String(value ?? '')
  return s.length > max ? `${s.slice(0, max)}…` : s
}

// Список -> карта «имя в нижнем регистре -> количество» (счётчики фасетов)
function toCountMap(list) {
  const map = {}
  ;(list ?? []).forEach((x) => {
    map[x.name.toLowerCase()] = x.count
  })
  return map
}

// Скачивание dataURL отдельным файлом
function downloadDataUrl(dataUrl, filename) {
  const link = document.createElement('a')
  link.href = dataUrl
  link.download = filename
  document.body.appendChild(link)
  link.click()
  link.remove()
}

// ---------- Экспорт PNG без внешних зависимостей ----------
// Recharts рендерит график в <svg> внутри контейнера. Клонируем SVG,
// добавляем белый фон и xmlns (иначе canvas откажется рисовать),
// сериализуем и растрируем в canvas с 2x масштабом (pixelRatio).
// Возвращает dataURL либо null, если в контейнере нет SVG
// (например, состояние «Нет данных за выбранный период»).
async function chartToPng(containerRef) {
  const container = containerRef?.current
  if (!container) return null
  const svg = container.querySelector('svg')
  if (!svg) return null

  const clone = svg.cloneNode(true)
  clone.setAttribute('xmlns', 'http://www.w3.org/2000/svg')
  // Стили корня (width/height: 100% и т.п.) сбрасываем: внутри <img>
  // проценты резолвятся иначе, из-за чего вокруг графика могли оставаться
  // прозрачные области (их подложка тёмного просмотрщика выглядела
  // как тёмные полосы). Размер задаём явными атрибутами ниже.
  clone.removeAttribute('style')

  // Размеры: приоритет у viewBox (ResponsiveContainer его задаёт),
  // запасной вариант — клиентские размеры элемента
  const viewBox = svg.viewBox?.baseVal
  const width =
    viewBox && viewBox.width ? viewBox.width : svg.clientWidth || 750
  const height =
    viewBox && viewBox.height ? viewBox.height : svg.clientHeight || 300
  clone.setAttribute('width', width)
  clone.setAttribute('height', height)

  // Шрифт: у сериализованного SVG нет CSS страницы — задаём явно,
  // чтобы подписи осей не схлопывались в дефолтный serif
  clone.setAttribute(
    'font-family',
    "'Rostelecom Basis', 'Segoe UI', system-ui, sans-serif",
  )

  // Цвета: Recharts задаёт fill/stroke через CSS-переменные
  // (var(--accent-default) и т.д.). Вне документа переменные
  // не резолвятся и заливка падает в чёрный. Копируем ВЫЧИСЛЕННЫЕ
  // значения (getComputedStyle уже развёрнул var() в rgb) в атрибуты
  // клона — только там, где атрибут действительно содержит var().
  const origNodes = [svg, ...svg.querySelectorAll('*')]
  const cloneNodes = [clone, ...clone.querySelectorAll('*')]
  origNodes.forEach((orig, i) => {
    const target = cloneNodes[i]
    if (!target) return
    const computed = getComputedStyle(orig)
    const attrFill = target.getAttribute('fill')
    const attrStroke = target.getAttribute('stroke')
    if (attrFill && attrFill.includes('var(') && computed.fill) {
      target.setAttribute('fill', computed.fill)
    }
    if (attrStroke && attrStroke.includes('var(') && computed.stroke) {
      target.setAttribute('stroke', computed.stroke)
    }
  })

  // Белый фон (прозрачный SVG на прозрачном canvas дал бы чёрный PNG)
  const rect = document.createElementNS('http://www.w3.org/2000/svg', 'rect')
  rect.setAttribute('x', 0)
  rect.setAttribute('y', 0)
  rect.setAttribute('width', width)
  rect.setAttribute('height', height)
  rect.setAttribute('fill', '#ffffff')
  clone.insertBefore(rect, clone.firstChild)

  const xml = new XMLSerializer().serializeToString(clone)
  const svgUrl =
    'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(xml)

  return new Promise((resolve, reject) => {
    const img = new Image()
    img.onload = () => {
      const scale = 2
      const canvas = document.createElement('canvas')
      canvas.width = Math.round(width * scale)
      canvas.height = Math.round(height * scale)
      const ctx = canvas.getContext('2d')
      ctx.fillStyle = '#ffffff'
      ctx.fillRect(0, 0, canvas.width, canvas.height)
      ctx.scale(scale, scale)
      ctx.drawImage(img, 0, 0)
      resolve(canvas.toDataURL('image/png'))
    }
    img.onerror = () =>
      reject(new Error('Не удалось отрендерить SVG графика в PNG.'))
    img.src = svgUrl
  })
}

// ---------- Сборка карточки графика: заголовок + снимок + легенда ----------
// Рисуем на canvas ПОВЕРХ снимка графика. Причина — кириллица:
// jsPDF печатает свой текст встроенным Helvetica, где кириллицы НЕТ
// (русский текст превращался в сырые коды символов вида "8 7 C 0;...").
// Canvas печатает русский системными шрифтами без ограничений, поэтому
// в PDF уходят только изображения. Легенда (для круговой диаграммы)
// тоже HTML вне SVG — добавляем её сюда же, иначе в файлах её не было.
// columns > 1 раскладывает легенду в несколько колонок (для PDF,
// чтобы карточка помещалась на лист A4).
function composeCardPng({ title, chartPng, legend = null, columns = 1 }) {
  return new Promise((resolve, reject) => {
    const img = new Image()
    img.onload = () => {
      const scale = 2
      const pad = 24
      const rowH = 30
      const titleH = 56
      const rows = legend ? Math.ceil(legend.length / columns) : 0
      const legendH = legend ? rows * rowH + 16 : 0
      const width = Math.max(img.width, 560)
      const height = titleH + img.height + legendH

      const canvas = document.createElement('canvas')
      canvas.width = width
      canvas.height = height
      const ctx = canvas.getContext('2d')
      ctx.fillStyle = '#ffffff'
      ctx.fillRect(0, 0, width, height)

      const FONT =
        "'Rostelecom Basis', 'Segoe UI', system-ui, sans-serif"

      // Заголовок карточки
      ctx.fillStyle = '#101014'
      ctx.font = `600 ${17 * scale}px ${FONT}`
      ctx.textBaseline = 'middle'
      ctx.textAlign = 'left'
      ctx.fillText(title, pad, titleH / 2)

      // Сам график
      ctx.drawImage(img, 0, titleH)

      // Легенда (колонками)
      if (legend) {
        const colW = (width - pad * 2) / columns
        legend.forEach((item, i) => {
          const col = Math.floor(i / rows)
          const row = i % rows
          const x = pad + col * colW
          const y = titleH + img.height + 22 + row * rowH

          ctx.fillStyle = item.color
          ctx.beginPath()
          ctx.arc(x + 6, y, 6, 0, Math.PI * 2)
          ctx.fill()

          ctx.fillStyle = '#45454d'
          ctx.font = `${12 * scale}px ${FONT}`
          ctx.textAlign = 'left'
          ctx.fillText(truncateLabel(item.name, 24), x + 22, y)

          ctx.fillStyle = '#101014'
          ctx.font = `700 ${12 * scale}px ${FONT}`
          ctx.textAlign = 'right'
          ctx.fillText(String(item.count), x + colW - 8, y)
          ctx.textAlign = 'left'
        })
      }

      resolve({ dataUrl: canvas.toDataURL('image/png'), width, height })
    }
    img.onerror = () =>
      reject(new Error('Не удалось собрать итоговое изображение карточки.'))
    img.src = chartPng
  })
}

// ---------- Главный компонент ----------

export default function VisualizationPage() {
  const { user } = useAuth()

  // ---------- Ролевая модель (п. 11 ТЗ) ----------
  // Все роли: фильтрация (включая «Ответственный») и экспорт PNG/PDF.
  // Разница только у администратора: + результирующий JSON.
  // Разграничение ВИДИМОСТИ данных (свои вузы) — на бэкенде
  // (university_managers), фронт только следует п. 11 ТЗ по кнопкам.
  const roles = user?.roles ?? []
  const isAdmin = roles.includes('admin')

  // ---------- Справочники ----------
  const [universities, setUniversities] = useState([])
  const [directions, setDirections] = useState([])
  const [programs, setPrograms] = useState([])
  const [products, setProducts] = useState([])
  const [interactions, setInteractions] = useState([])
  const [managers, setManagers] = useState([])
  const [pageError, setPageError] = useState('')

  // ---------- Статистика ----------
  // statistics      — агрегаты по ТЕКУЩИМ фильтрам (диаграмма по направлениям)
  // menuStatistics  — агрегаты БЕЗ фильтров (списки пунктов не исчезают)
  const [statistics, setStatistics] = useState(null)
  const [menuStatistics, setMenuStatistics] = useState(null)
  const [uniFacetCounts, setUniFacetCounts] = useState({})
  const [dirFacetCounts, setDirFacetCounts] = useState({})
  const [prodFacetCounts, setProdFacetCounts] = useState({})
  const [loading, setLoading] = useState(true)

  // ---------- Период и фильтры (мультивыбор), с кэшем в localStorage ----------
  const [period, setPeriod] = useState(
    () => localStorage.getItem(LS_PERIOD) ?? 'month',
  )
  const [periodOpen, setPeriodOpen] = useState(false)
  const [filterOpen, setFilterOpen] = useState(false)
  const [selectedUniversityIds, setSelectedUniversityIds] = useState(() =>
    readStoredIds(LS_UNIVERSITIES),
  )
  const [selectedDirectionIds, setSelectedDirectionIds] = useState(() =>
    readStoredIds(LS_DIRECTIONS),
  )
  const [selectedProductIds, setSelectedProductIds] = useState(() =>
    readStoredIds(LS_PRODUCTS),
  )
  const [selectedManagerIds, setSelectedManagerIds] = useState(() =>
    readStoredIds(LS_MANAGERS),
  )

  // ---------- Экспорт ----------
  const [exportOpen, setExportOpen] = useState(false)
  const [exporting, setExporting] = useState('') // 'png' | 'pdf' | 'json' | ''
  const [exportError, setExportError] = useState('')
  const [exportNotice, setExportNotice] = useState('')

  // ---------- Ref-графиков для снимков PNG ----------
  // Обычные ref на контейнеры графиков — chartToPng достаёт из них SVG
  const refMonthly = useRef(null)
  const refDirections = useRef(null)
  const refFlows = useRef(null)

  const currentPeriod = PERIODS.find((p) => p.id === period) ?? PERIODS[2]

  // ---------- Загрузка справочников и взаимодействий (один раз) ----------
  useEffect(() => {
    setLoading(true)
    Promise.all([
      api.get('/Universities'),
      api.get('/Directions'),
      api.get('/Programs'),
      api.get('/Products'),
      api.get('/Interactions'),
    ])
      .then(([unis, dirs, progs, prods, list]) => {
        setUniversities(unis ?? [])
        setDirections(dirs ?? [])
        setPrograms(progs ?? [])
        setProducts(prods ?? [])
        setInteractions(list ?? [])
      })
      .catch((err) => setPageError(err.message))
      .finally(() => setLoading(false))

    // Список пользователей для фильтра «Ответственный» —
    // по ТЗ доступен всем ролям
    api
      .get('/Users')
      .then((list) => setManagers((list ?? []).filter((u) => u.isActive !== false)))
      .catch(() => setManagers([]))
  }, [])

  // ---------- Кэш действий пользователя (нефункц. требование 13) ----------
  useEffect(() => {
    localStorage.setItem(LS_PERIOD, period)
  }, [period])

  useEffect(() => {
    localStorage.setItem(LS_UNIVERSITIES, JSON.stringify(selectedUniversityIds))
  }, [selectedUniversityIds])

  useEffect(() => {
    localStorage.setItem(LS_DIRECTIONS, JSON.stringify(selectedDirectionIds))
  }, [selectedDirectionIds])

  useEffect(() => {
    localStorage.setItem(LS_PRODUCTS, JSON.stringify(selectedProductIds))
  }, [selectedProductIds])

  useEffect(() => {
    localStorage.setItem(LS_MANAGERS, JSON.stringify(selectedManagerIds))
  }, [selectedManagerIds])

  // ---------- Фильтр для API статистики ----------
  // managerIds в статистику не передаём: клиент отфильтрует по менеджерам
  // самостоятельно (эндпоинт /Reports/statistics принимает вузы,
  // направления и продукты — см. DashboardPage/ReportsPage).
  const filter = useMemo(() => {
    const start = periodStart(currentPeriod.id)
    return {
      dateFrom: start ? start.toISOString() : null,
      universityIds: selectedUniversityIds,
      directionIds: selectedDirectionIds,
      productIds: selectedProductIds,
    }
  }, [currentPeriod, selectedUniversityIds, selectedDirectionIds, selectedProductIds])

  // ---------- Основная статистика (с фильтрами) ----------
  useEffect(() => {
    const body = { ...filter }
    if (body.dateFrom == null) delete body.dateFrom
    getStatistics(body)
      .then(setStatistics)
      .catch((err) => setPageError(err.message))
  }, [filter])

  // ---------- Списки для меню фильтров: пересчитываются при смене
  // периода без учёта выбранных фильтров, чтобы пункты не исчезали ----------
  useEffect(() => {
    const start = periodStart(currentPeriod.id)
    const body = start ? { dateFrom: start.toISOString() } : {}
    getStatistics(body)
      .then(setMenuStatistics)
      .catch(() => {})
  }, [currentPeriod])

  // ---------- Счётчики пунктов меню: каждая секция считается БЕЗ
  // фильтра своей секции, но С остальными фильтрами ----------
  useEffect(() => {
    const start = periodStart(currentPeriod.id)
    const base = start ? { dateFrom: start.toISOString() } : {}

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

  // ---------- Клиентская фильтрация взаимодействий ----------
  const programDirectionMap = useMemo(() => {
    const map = {}
    programs.forEach((p) => {
      map[p.id] = p.directionId
    })
    return map
  }, [programs])

  const filtered = useMemo(() => {
    const from = periodStart(currentPeriod.id)?.getTime() ?? null
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
      // Фильтр по ответственным — для всех ролей (по ТЗ);
      // бэкенд уже ограничил выборку вузами пользователя
      if (
        selectedManagerIds.length &&
        !selectedManagerIds.includes(i.managerId)
      ) {
        return false
      }
      const time = i.createdAt ? new Date(i.createdAt).getTime() : 0
      return from == null || time >= from
    })
  }, [
    interactions,
    currentPeriod,
    selectedUniversityIds,
    selectedDirectionIds,
    selectedProductIds,
    selectedManagerIds,
    programDirectionMap,
  ])

  // ---------- График 1: Заявки на обучения по месяцам ----------
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

  // ---------- Диаграмма 2: Количество обучающихся по направлениям ----------
  // Источник — агрегат бэкенда по текущим фильтрам; обезличенно:
  // количество обучающихся = суммарное число заявок/взаимодействий
  // по каждому направлению. Показываем ВСЕ направления выборки —
  // ограничивать список нельзя (финальная экспертиза: достоверность
  // диаграмм оценивается по отражению данных).
  const byDirection = useMemo(
    () => statistics?.byDirection ?? [],
    [statistics],
  )

  // Легенда круговой диаграммы для экспорта: имена + счётчики + цвета
  // (те же, что при рендере сегментов)
  const directionLegend = useMemo(
    () =>
      byDirection.map((d, i) => ({
        name: d.name,
        count: d.count,
        color: PIE_COLORS[i % PIE_COLORS.length],
      })),
    [byDirection],
  )

  // ---------- График 3: Количество параллельных потоков ----------
  // Поток = программа, ведущаяся у вуза в рамках взаимодействия.
  // Параллельные потоки по направлению = число одновременно идущих
  // взаимодействий (вуз × программа) этого направления.
  const flowsByDirection = useMemo(() => {
    const map = {}
    filtered.forEach((i) => {
      const directionId = programDirectionMap[i.programId]
      if (directionId == null) return
      if (!map[directionId]) {
        map[directionId] = { key: directionId, name: null, count: 0 }
      }
      map[directionId].count += 1
    })
    return Object.values(map)
      .map((entry) => ({
        ...entry,
        name:
          directions.find((d) => d.id === entry.key)?.name ??
          `Направление #${entry.key}`,
      }))
      .sort((a, b) => b.count - a.count)
  }, [filtered, programDirectionMap, directions])

  // ---------- Подписи чипов выбранных фильтров ----------
  const universityChip = (id) => {
    const u = universities.find((x) => x.id === id)
    return u ? u.shortName || u.name : `Вуз #${id}`
  }
  const directionChip = (id) => directions.find((x) => x.id === id)?.name ?? `Направление #${id}`
  const productChip = (id) => products.find((x) => x.id === id)?.name ?? `Продукт #${id}`
  const managerChip = (id) => {
    const u = managers.find((x) => x.id === id)
    if (!u) return `Ответственный #${id}`
    const name = [u.lastName, u.firstName, u.middleName].filter(Boolean).join(' ').trim()
    return name || u.fullName || u.email
  }

  const hasAnyChip =
    selectedUniversityIds.length > 0 ||
    selectedDirectionIds.length > 0 ||
    selectedProductIds.length > 0 ||
    selectedManagerIds.length > 0

  function toggleId(list, setList, id) {
    setList(list.includes(id) ? list.filter((x) => x !== id) : [...list, id])
  }

  // ---------- Экспорт ----------
  // Пользователь выбирает: ВСЕ графики или ОДИН конкретный
  // (выборочная выгрузка удобна для презентаций/отчётов руководству).
  // Форматы — 2 варианта по макету: PNG, PDF (+ JSON — администратор,
  // п. 11 ТЗ и п. 4 «Требований к решению»).

  // Единый реестр графиков: ключ -> ref контейнера, заголовок карточки,
  // имя файла, легенда (только круговая диаграмма направлений).
  // Определён ПОСЛЕ directionLegend: использует её в поле legend
  // (объявление раньше вызвало бы TDZ-ошибку).
  const chartCards = [
    {
      key: 'monthly',
      ref: refMonthly,
      title: 'Заявки на обучения по месяцам',
      file: 'viz-zayavki',
    },
    {
      key: 'directions',
      ref: refDirections,
      title: 'Количество обучающихся по направлениям',
      file: 'viz-napravleniya',
      legend: directionLegend,
    },
    {
      key: 'flows',
      ref: refFlows,
      title: 'Количество параллельных потоков',
      file: 'viz-potoki',
    },
  ]

  // selection: 'all' | ключ графика из chartCards
  async function exportPng(selection = 'all') {
    setExporting('png')
    setExportError('')
    setExportNotice('')
    try {
      const stamp = new Date().toISOString().slice(0, 10)
      const targets =
        selection === 'all'
          ? chartCards
          : chartCards.filter((c) => c.key === selection)

      let exported = 0
      // Последовательно: параллельные скачивания часто блокируются
      // браузером как всплывающие окна
      for (const chart of targets) {
        const raw = await chartToPng(chart.ref)
        if (!raw) continue
        const card = await composeCardPng({
          title: chart.title,
          chartPng: raw,
          legend: chart.legend,
        })
        downloadDataUrl(card.dataUrl, `${chart.file}-${stamp}.png`)
        exported += 1
      }

      if (exported === 0) {
        throw new Error('Не удалось построить снимки графиков.')
      }
      setExportNotice(
        exported === 1
          ? 'График выгружен в PNG.'
          : `Выгружено графиков: ${exported}.`,
      )
    } catch (err) {
      setExportError(err.message ?? 'Ошибка экспорта PNG.')
    } finally {
      setExporting('')
      setExportOpen(false)
    }
  }

  // selection: 'all' | ключ графика из chartCards
  async function exportPdf(selection = 'all') {
    setExporting('pdf')
    setExportError('')
    setExportNotice('')
    try {
      const targets =
        selection === 'all'
          ? chartCards
          : chartCards.filter((c) => c.key === selection)

      // jsPDF подключаем динамически: если пакет не установлен,
      // страница работает, а здесь покажем понятную ошибку
      let jsPDF
      try {
        ;({ jsPDF } = await import('jspdf'))
      } catch {
        throw new Error('Пакет jspdf не установлен. Выполните: npm i jspdf')
      }

      // Карточки с заголовками. Легенда диаграммы — в 2 колонки,
      // иначе карточка с 10 направлениями не помещается на лист A4.
      // Текста через pdf.text НЕТ нигде: встроенный шрифт jsPDF
      // не содержит кириллицу (печатал коды символов) — весь текст
      // уже нарисован на изображениях карточек.
      const built = []
      for (const chart of targets) {
        const raw = await chartToPng(chart.ref)
        if (!raw) continue
        const card = await composeCardPng({
          title: `${chart.title} (${chart.legend ? 'диаграмма' : 'график'})`,
          chartPng: raw,
          legend: chart.legend,
          columns: chart.legend ? 2 : 1,
        })
        built.push(card)
      }
      if (built.length === 0) {
        throw new Error('Не удалось построить снимки графиков.')
      }

      // Лист A4 альбомной ориентации: 842 x 595 pt.
      // Карточки друг под другом; реальные пропорции изображений.
      const stamp = new Date().toISOString().slice(0, 10)
      const pdf = new jsPDF({ orientation: 'landscape', unit: 'pt', format: 'a4' })
      const pageWidth = pdf.internal.pageSize.getWidth()
      const pageHeight = pdf.internal.pageSize.getHeight()
      const marginX = 32
      const contentWidth = pageWidth - marginX * 2
      const startY = 28
      const bottomLimit = pageHeight - 20
      let cursorY = startY

      built.forEach((card) => {
        const imgHeight = Math.round(contentWidth * (card.height / card.width))
        // Если карточка не помещается на текущий лист — новая страница
        if (cursorY + imgHeight > bottomLimit) {
          pdf.addPage()
          cursorY = startY
        }
        pdf.addImage(card.dataUrl, 'PNG', marginX, cursorY, contentWidth, imgHeight)
        cursorY += imgHeight + 18
      })

      pdf.save(`viz-${stamp}.pdf`)
      setExportNotice('Отчёт выгружен в PDF.')
    } catch (err) {
      setExportError(err.message ?? 'Ошибка экспорта PDF.')
    } finally {
      setExporting('')
      setExportOpen(false)
    }
  }

  function exportJson() {
    setExporting('json')
    setExportError('')
    setExportNotice('')
    try {
      const stamp = new Date().toISOString().slice(0, 10)
      const payload = {
        generatedAt: new Date().toISOString(),
        period: currentPeriod.id,
        filters: {
          universityIds: selectedUniversityIds,
          directionIds: selectedDirectionIds,
          productIds: selectedProductIds,
          managerIds: selectedManagerIds,
        },
        charts: {
          monthlyApplications: monthly,
          studentsByDirection: byDirection,
          parallelFlowsByDirection: flowsByDirection,
        },
      }
      const blob = new Blob([JSON.stringify(payload, null, 2)], {
        type: 'application/json;charset=utf-8',
      })
      const url = URL.createObjectURL(blob)
      downloadDataUrl(url, `viz-${stamp}.json`)
      URL.revokeObjectURL(url)
      setExportNotice('Данные визуализации выгружены в JSON.')
    } catch (err) {
      setExportError(err.message ?? 'Ошибка экспорта JSON.')
    } finally {
      setExporting('')
      setExportOpen(false)
    }
  }

  return (
    <div className="dashboard">
      {/* ---------- Шапка по макету: «Визуализация данных» + [Экспорт ▾].
           При нажатии на «Экспорт» появляются 2 варианта форматов
           (PNG, PDF) с выбором: все графики или один конкретный
           (+ JSON — расширенная привилегия администратора, п. 11 ТЗ) ---------- */}
      <div className="viz-head panel">
        <h1 className="viz-title">Визуализация данных</h1>

        <div className="page-actions">
          {/* Выбор периода */}
          <div className="menu-wrap">
            <button
              className="btn-ghost"
              onClick={() => {
                setPeriodOpen((v) => !v)
                setFilterOpen(false)
                setExportOpen(false)
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

          {/* Фильтры: вузы / направления / продукты / ответственные —
              всем ролям (в ТЗ фильтр по ответственным не делится
              по ролям; пользователь формирует отчёты «по выбранным …
              ответственным за них») */}
          <div className="menu-wrap">
            <button
              className="btn-ghost"
              onClick={() => {
                setFilterOpen((v) => !v)
                setPeriodOpen(false)
                setExportOpen(false)
              }}
            >
              Фильтры
              <IconChevronDown size={16} />
            </button>
            {filterOpen && (
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
                      onClick={() =>
                        id != null &&
                        toggleId(selectedUniversityIds, setSelectedUniversityIds, id)
                      }
                    >
                      <span className="menu-text">{u.name}</span>
                      {active ? (
                        <span className="menu-check">✓</span>
                      ) : (
                        <span className={count === 0 ? 'menu-count is-zero' : 'menu-count'}>
                          {count}
                        </span>
                      )}
                    </button>
                  )
                })}
                {selectedUniversityIds.length > 0 && (
                  <button
                    className="menu-item menu-clear"
                    onClick={() => setSelectedUniversityIds([])}
                  >
                    Сбросить выбор вузов
                  </button>
                )}

                <div className="menu-group">ИТ-направления</div>
                {(menuStatistics?.byDirection ?? []).slice(0, 8).map((d) => {
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
                      onClick={() =>
                        id != null &&
                        toggleId(selectedDirectionIds, setSelectedDirectionIds, id)
                      }
                    >
                      <span className="menu-text">{d.name}</span>
                      {active ? (
                        <span className="menu-check">✓</span>
                      ) : (
                        <span className={count === 0 ? 'menu-count is-zero' : 'menu-count'}>
                          {count}
                        </span>
                      )}
                    </button>
                  )
                })}
                {selectedDirectionIds.length > 0 && (
                  <button
                    className="menu-item menu-clear"
                    onClick={() => setSelectedDirectionIds([])}
                  >
                    Сбросить выбор направлений
                  </button>
                )}

                <div className="menu-group">ИТ-продукты</div>
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
                      onClick={() =>
                        id != null &&
                        toggleId(selectedProductIds, setSelectedProductIds, id)
                      }
                    >
                      <span className="menu-text">{p.name}</span>
                      {active ? (
                        <span className="menu-check">✓</span>
                      ) : (
                        <span className={count === 0 ? 'menu-count is-zero' : 'menu-count'}>
                          {count}
                        </span>
                      )}
                    </button>
                  )
                })}
                {selectedProductIds.length > 0 && (
                  <button
                    className="menu-item menu-clear"
                    onClick={() => setSelectedProductIds([])}
                  >
                    Сбросить выбор продуктов
                  </button>
                )}

                {/* Фильтр «Ответственный» — все роли (по ТЗ) */}
                <>
                    <div className="menu-group">Ответственные</div>
                    {managers.slice(0, 10).map((m) => {
                      const active = selectedManagerIds.includes(m.id)
                      return (
                        <button
                          key={m.id}
                          className={active ? 'menu-item active' : 'menu-item'}
                          onClick={() =>
                            toggleId(selectedManagerIds, setSelectedManagerIds, m.id)
                          }
                        >
                          <span className="menu-text">{managerChip(m.id)}</span>
                          {active && <span className="menu-check">✓</span>}
                        </button>
                      )
                    })}
                    {selectedManagerIds.length > 0 && (
                      <button
                        className="menu-item menu-clear"
                        onClick={() => setSelectedManagerIds([])}
                      >
                        Сбросить выбор ответственных
                      </button>
                    )}
                </>
              </div>
            )}
          </div>

          {/* Кнопка «Экспорт»: при нажатии появляются варианты —
              PNG, PDF (стрелка на макете): все графики или один.
              JSON — только администратор. */}
          <div className="menu-wrap">
            <button
              className="btn-accent-soft"
              onClick={() => {
                setExportOpen((v) => !v)
                setPeriodOpen(false)
                setFilterOpen(false)
              }}
              disabled={exporting !== ''}
            >
              <IconDownload size={16} />
              {exporting ? 'Экспорт…' : 'Экспорт'}
              <IconChevronDown size={16} />
            </button>
            {exportOpen && (
              <div className="menu menu--wide">
                {/* PNG: всё или отдельный график */}
                <div className="menu-group">PNG</div>
                <button
                  className="menu-item"
                  onClick={() => exportPng('all')}
                  disabled={exporting !== ''}
                >
                  <span className="menu-text">Все графики</span>
                  <span className="menu-count">3 файла</span>
                </button>
                {chartCards.map((chart) => (
                  <button
                    key={chart.key}
                    className="menu-item"
                    onClick={() => exportPng(chart.key)}
                    disabled={exporting !== ''}
                  >
                    <span className="menu-text">{chart.title}</span>
                  </button>
                ))}

                {/* PDF: всё или отдельный график */}
                <div className="menu-group">PDF</div>
                <button
                  className="menu-item"
                  onClick={() => exportPdf('all')}
                  disabled={exporting !== ''}
                >
                  <span className="menu-text">Все графики</span>
                  <span className="menu-count">1 файл</span>
                </button>
                {chartCards.map((chart) => (
                  <button
                    key={chart.key}
                    className="menu-item"
                    onClick={() => exportPdf(chart.key)}
                    disabled={exporting !== ''}
                  >
                    <span className="menu-text">{chart.title}</span>
                  </button>
                ))}

                {/* Результирующий JSON — расширенная привилегия
                    администратора (п. 11 ТЗ + п. 4 требований к решению) */}
                {isAdmin && (
                  <>
                    <div className="menu-group">Данные</div>
                    <button
                      className="menu-item"
                      onClick={exportJson}
                      disabled={exporting !== ''}
                    >
                      <span className="menu-text">JSON (все графики)</span>
                      <IconReport size={14} />
                    </button>
                  </>
                )}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* ---------- Строка чипов применённых фильтров ---------- */}
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
          {selectedManagerIds.map((id) => (
            <button
              key={`mgr-${id}`}
              className="chip"
              onClick={() => toggleId(selectedManagerIds, setSelectedManagerIds, id)}
              title="Убрать фильтр"
            >
              <span className="chip-text">{managerChip(id)}</span> ✕
            </button>
          ))}
        </div>
      )}

      {pageError && <div className="form-error" role="alert">{pageError}</div>}
      {exportError && <div className="form-error" role="alert">{exportError}</div>}
      {exportNotice && <div className="form-success" role="status">{exportNotice}</div>}

      {/* ---------- Графики строго по макету ---------- */}
      {loading ? (
        <p className="page-loader">Загрузка…</p>
      ) : (
        <div className="viz-grid">
          {/* График 1: Заявки на обучения по месяцам (левая колонка).
              height="100%" — график заполняет панель целиком, панели
              первой строки одинаковой высоты (см. .viz-grid в CSS) */}
          <section className="panel">
            <h2>Заявки на обучения по месяцам</h2>
            <p className="panel-sub">(график)</p>
            <div className="chart" ref={refMonthly}>
              {monthly.length === 0 ? (
                <p className="empty">Нет данных за выбранный период</p>
              ) : (
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={monthly}
                    margin={{ top: 8, right: 8, left: -18, bottom: 0 }}
                  >
                    <CartesianGrid
                      strokeDasharray="3 3"
                      stroke="var(--border-soft)"
                      vertical={false}
                    />
                    <XAxis
                      dataKey="name"
                      tick={{ fontSize: 12, fill: 'var(--fg-muted)' }}
                      axisLine={false}
                      tickLine={false}
                    />
                    <YAxis
                      tick={{ fontSize: 12, fill: 'var(--fg-muted)' }}
                      axisLine={false}
                      tickLine={false}
                      allowDecimals={false}
                    />
                    <Tooltip cursor={{ fill: 'var(--accent-bg)' }} />
                    <Bar
                      dataKey="count"
                      name="Заявки"
                      fill="var(--accent-default)"
                      radius={[6, 6, 0, 0]}
                      maxBarSize={42}
                    />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </div>
          </section>

          {/* График 2: Количество обучающихся по направлениям (правая колонка) */}
          <section className="panel">
            <h2>Количество обучающихся по направлениям</h2>
            <p className="panel-sub">(диаграмма)</p>
            <div className="chart" ref={refDirections}>
              {byDirection.length === 0 ? (
                <p className="empty">Нет данных за выбранный период</p>
              ) : (
                <>
                  <ResponsiveContainer width="100%" height={240}>
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
                          <Cell
                            key={entry.name}
                            fill={PIE_COLORS[index % PIE_COLORS.length]}
                          />
                        ))}
                      </Pie>
                      <Tooltip />
                    </PieChart>
                  </ResponsiveContainer>
                  <ul className="pie-legend">
                    {byDirection.map((entry, index) => (
                      <li key={entry.name}>
                        <span
                          className="pie-dot"
                          style={{ background: PIE_COLORS[index % PIE_COLORS.length] }}
                        />
                        {entry.name}
                        <strong>{entry.count}</strong>
                      </li>
                    ))}
                  </ul>
                </>
              )}
            </div>
          </section>

          {/* График 3: Количество параллельных потоков — на всю ширину.
              Горизонтальные столбцы: длинные названия направлений читаются
              ПОЛНОСТЬЮ на оси категорий (вертикальная ось с поворотом
              подписей их обрезала). Высота подстраивается под число
              направлений, чтобы не было пустот или скролла. */}
          <section className="panel panel--wide">
            <h2>Количество параллельных потоков</h2>
            <p className="panel-sub">(график)</p>
            <div className="chart" ref={refFlows}>
              {flowsByDirection.length === 0 ? (
                <p className="empty">Нет данных за выбранный период</p>
              ) : (
                <ResponsiveContainer
                  width="100%"
                  height={Math.max(240, flowsByDirection.length * 34 + 60)}
                >
                  <BarChart
                    layout="vertical"
                    data={flowsByDirection}
                    margin={{ top: 8, right: 32, left: 8, bottom: 8 }}
                    barCategoryGap="28%"
                  >
                    <CartesianGrid
                      strokeDasharray="3 3"
                      stroke="var(--border-soft)"
                      horizontal={false}
                    />
                    <XAxis
                      type="number"
                      tick={{ fontSize: 12, fill: 'var(--fg-muted)' }}
                      axisLine={false}
                      tickLine={false}
                      allowDecimals={false}
                    />
                    <YAxis
                      type="category"
                      dataKey="name"
                      width={200}
                      tick={{ fontSize: 12, fill: 'var(--fg-soft)' }}
                      axisLine={false}
                      tickLine={false}
                    />
                    <Tooltip cursor={{ fill: 'var(--accent-bg)' }} />
                    <Bar
                      dataKey="count"
                      name="Потоки"
                      fill="var(--status-03-default)"
                      radius={[0, 6, 6, 0]}
                      maxBarSize={26}
                    />
                  </BarChart>
                </ResponsiveContainer>
              )}
            </div>
          </section>
        </div>
      )}
    </div>
  )
}