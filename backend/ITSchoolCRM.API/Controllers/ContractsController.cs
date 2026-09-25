// =====================================================================
// Контроллер раздела «Договоры» — тонкий, в едином стиле проекта
// (образец — ProductsController): только авторизация политиками,
// вызов сервиса и HTTP-обёртка результата. Никакой работы с claims
// на контроллере нет — текущий пользователь известен сервисам аудита
// и авторизации самостоятельно.
//
// Покрытие ТЗ:
// • п. 11 ТЗ (ролевая модель): чтение — Policies.UserAccess (все
//   авторизованные), создание/изменение — Policies.ManagerAccess
//   (manager/admin; политика уже зарегистрирована в проекте —
//   используется ProductsController);
// • п. 2 «Требований к решению»: все методы доступны и описаны
//   через Swagger UI автоматически;
// • нефункц. требование 3: ошибки сервиса (ArgumentException ->
//   ERR_VALIDATION, InvalidOperationException -> ERR_CONFLICT)
//   обрабатываются существующим конвейером проекта; «не найдено»
//   — стандартный 404, как у ProductsController/DirectionsController.
//
// ВАЖНО про маршруты: "interaction-options" — литеральный сегмент,
// GET-метода с {id:int} нет — конфликтов маршрутизации не возникает.
//
// Контракт с фронтендом (строго по api/contracts.js):
//   GET  /api/Contracts                  -> List<ContractDto>
//   GET  /api/Contracts/interaction-options -> List<ContractInteractionOptionDto>
//   POST /api/Contracts                  -> ContractDto
//   PUT  /api/Contracts/{id}             -> ContractDto (404, если нет)
// =====================================================================

using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Contracts;
using ITSchoolCRM.API.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _service;

    public ContractsController(
        IContractService service)
    {
        _service = service;
    }

    // -----------------------------------------------------------------
    // GET /api/Contracts — реестр договоров.
    // Используется страницей «Договоры» и (колонка «№ Договора»)
    // страницами «Вузы» и «Отчёты». Реестр общий для всех ролей —
    // как у справочников Products/Directions (п. 11 ТЗ: пользователь
    // работает с допустимыми в рамках прав данными; селективное
    // ограничение по university_marrants здесь не дублируется).
    // -----------------------------------------------------------------
    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<ContractDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var contracts =
            await _service.GetAllAsync(
                cancellationToken);

        return Ok(contracts);
    }

    // -----------------------------------------------------------------
    // GET /api/Contracts/interaction-options — взаимодействия БЕЗ
    // договора (interactions.contract_id IS NULL). Список для селекта
    // «Вуз / ПО» в диалоге «Добавить договор» на фронте: выбранное
    // взаимодействие будет привязано к новому договору в одной
    // транзакции (CreateAsync).
    // -----------------------------------------------------------------
    [HttpGet("interaction-options")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<ContractInteractionOptionDto>>> GetInteractionOptions(
        CancellationToken cancellationToken)
    {
        var options =
            await _service.GetInteractionOptionsAsync(
                cancellationToken);

        return Ok(options);
    }

    // -----------------------------------------------------------------
    // POST /api/Contracts — создание договора (+ опциональная привязка
    // к взаимодействию в одной транзакции). Только manager/admin.
    // POST возвращает созданный DTO (тот же тип, что у GET) — фронт
    // после создания всё равно делает полную перезагрузку реестра,
    // но единообразие типов ответа упрощает Swagger-документацию.
    //
    // Ошибки сервиса (обрабатывает конвейер проекта, ErrorResponseDto):
    //   ArgumentException         — 400 ERR_VALIDATION (пустой номер,
    //                               срок раньше подписания, взаимодействие
    //                               не найдено);
    //   InvalidOperationException — 409 ERR_CONFLICT (дубликат номера,
    //                               взаимодействие уже привязано).
    // -----------------------------------------------------------------
    [HttpPost]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<ActionResult<ContractDto>> Create(
        [FromBody] ContractCreateDto dto,
        CancellationToken cancellationToken)
    {
        var contract =
            await _service.CreateAsync(
                dto,
                cancellationToken);

        return Ok(contract);
    }

    // -----------------------------------------------------------------
    // PUT /api/Contracts/{id} — редактирование реквизитов договора
    // (вуз/ПО не меняются — только через привязку взаимодействий).
    // Только manager/admin (п. 11 ТЗ, политика ManagerAccess).
    // «Не найдено» — стандартный 404, как у остальных контроллеров.
    // -----------------------------------------------------------------
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<ActionResult<ContractDto>> Update(
        int id,
        [FromBody] ContractUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var updated =
            await _service.UpdateAsync(
                id,
                dto,
                cancellationToken);

        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }
}