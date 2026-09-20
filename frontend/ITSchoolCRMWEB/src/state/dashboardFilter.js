// Мини-шина событий для фильтра главной страницы.
// Верхняя панель (поиск по вузам) живёт вне DashboardPage, поэтому
// выбранный вуз передаётся сюда — дашборд подписывается и применяет фильтр.

let pendingUniversityId = null
const listeners = new Set()

// Запросить фильтр по вузу (вызывается из поиска в Topbar)
export function requestUniversityFilter(universityId) {
  pendingUniversityId = universityId
  listeners.forEach((cb) => cb(universityId))
}

// Забрать фильтр, ожидающий применения (DashboardPage делает это при монтировании)
export function consumePendingUniversityFilter() {
  const id = pendingUniversityId
  pendingUniversityId = null
  return id
}

// Подписаться на фильтры (DashboardPage)
export function onUniversityFilter(callback) {
  listeners.add(callback)
  return () => listeners.delete(callback)
}