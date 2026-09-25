// API для страницы «Ответственные».
// Менеджеры школы работают через существующую связку users - university_managers,
// а представители вузов — через существующую таблицу university_contacts.
//
// Контракт с бэкендом (ITSchoolCRM.API, ResponsiblesController):
// GET    /Responsibles/managers                        — менеджеры школы с закреплёнными вузами
// PUT    /Responsibles/managers/{userId}/universities  — полная замена закрепления (manager/admin)
// GET    /Responsibles/contacts                        — каталог представителей вузов
// POST   /Responsibles/contacts                        — создать представителя (manager/admin)
// PUT    /Responsibles/contacts/{id}                   — изменить/активировать (manager/admin)
// DELETE /Responsibles/contacts/{id}                   — мягкое удаление, is_active=false (manager/admin)

export async function getResponsibleManagers(api) {
  return api.get('/Responsibles/managers')
}

export async function updateManagerUniversities(api, userId, universityIds) {
  return api.put(`/Responsibles/managers/${userId}/universities`, {
    universityIds,
  })
}

export async function getUniversityContacts(api) {
  return api.get('/Responsibles/contacts')
}

export async function createUniversityContact(api, payload) {
  return api.post('/Responsibles/contacts', payload)
}

export async function updateUniversityContact(api, id, payload) {
  return api.put(`/Responsibles/contacts/${id}`, payload)
}

export async function deleteUniversityContact(api, id) {
  return api.del(`/Responsibles/contacts/${id}`)
}