// =====================================================================
// Контракт сервиса раздела «Договоры» — в едином стиле проекта
// (образец — IProductService/IDirectionService): методы принимают
// только DTO/id и CancellationToken. Никаких параметров текущего
// пользователя — аудит (IAuditService) сам знает актора, а реестр
// договоров общий для всех ролей, как у справочников.
//
// Домовенные коды ошибок (как в DirectionService):
//   ArgumentException         — валидация полей (номер пустой, срок
//                               раньше подписания, взаимодействие
//                               не найдено);
//   InvalidOperationException — конфликт (дубликат номера, взаимодействие
//                               уже привязано к другому договору);
//   null в UpdateAsync        — объект не найден (контроллер отдаёт 404).
//
// Контракт с фронтендом (строго по api/contracts.js):
//   GET  /Contracts                 -> GetAllAsync
//   GET  /Contracts/interaction-options -> GetInteractionOptionsAsync
//   POST /Contracts                 -> CreateAsync
//   PUT  /Contracts/{id}            -> UpdateAsync (null = не найден)
//
// Модель данных (уточнение заказчика от 16.09.2026 12:14): один
// договор может покрывать НЕСКОЛЬКО продуктов; вузы и продукты НЕ
// хранятся в contracts — агрегируются из взаимодействий
// (interactions.contract_id), поэтому в ContractDto — списки с DISTINCT.
// =====================================================================

using ITSchoolCRM.API.DTOs.Contracts;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IContractService
{
    // Реестр договоров — колонка «№ Договора» на странице «Вузы»
    // и таблица раздела «Договоры»
    Task<List<ContractDto>> GetAllAsync(
        CancellationToken cancellationToken);

    // Взаимодействия БЕЗ договора (interactions.contract_id IS NULL) —
    // кандидаты на привязку в диалоге «Добавить договор»
    Task<List<ContractInteractionOptionDto>> GetInteractionOptionsAsync(
        CancellationToken cancellationToken);

    // Создание договора (+ опциональная привязка к взаимодействию
    // в одной транзакции) — manager/admin
    Task<ContractDto> CreateAsync(
        ContractCreateDto dto,
        CancellationToken cancellationToken);

    // Редактирование реквизитов договора (вуз/ПО не меняются —
    // только через привязку взаимодействий) — manager/admin.
    // null = договор не найден (контроллер отдаёт 404)
    Task<ContractDto?> UpdateAsync(
        int id,
        ContractUpdateDto dto,
        CancellationToken cancellationToken);
}