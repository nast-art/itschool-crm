// =====================================================================
// API-хелперы страницы «Договоры».
//
// Контракт с бэкендом (ITSchoolCRM.API, ContractsController):
//   GET  /Contracts                 — реестр договоров (DTO с агрегированными
//                                     вузами и продуктами из взаимодействий)
//   GET  /Contracts/interaction-options — взаимодействия БЕЗ договора
//                                     (кандидаты на привязку при создании)
//   POST /Contracts                 — создание договора (+ опциональная
//                                     привязка к взаимодействию)
//   PUT  /Contracts/{id}            — редактирование договора
//
// Ролевая модель (п. 11 ТЗ): чтение — все авторизованные; создание и
// изменение — manager/admin (на бэкенне [Authorize(Roles = "manager,admin")],
// на фронте кнопки дополнительно скрыты — дублирование безопасности).
// =====================================================================

export async function getContracts(api) {
  return api.get('/Contracts')
}

export async function getContractInteractionOptions(api) {
  return api.get('/Contracts/interaction-options')
}

export async function createContract(api, payload) {
  return api.post('/Contracts', payload)
}

export async function updateContract(api, id, payload) {
  return api.put(`/Contracts/${id}`, payload)
}