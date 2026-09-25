namespace ITSchoolCRM.API.DTOs.Reports;

/// <summary>
/// Фильтр отчёта по взаимодействиям.
/// Имена свойств согласованы с фронтендом (ReportsPage.jsx, buildReportPayload):
/// сериализатор ASP.NET Core по умолчанию работает в camelCase, поэтому
/// json-поле "managerIds" маппится на ManagerIds, "universityIds" — на
/// UniversityIds и т.д. Расхождение имён — причина прежнего бага, когда
/// фильтры применялись молча неправильно или не применялись вовсе.
/// </summary>
public class ReportFilterDto
{
    /// <summary>Начало периода (включительно). ISO-строка с фронта.</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>Конец периода (включительно, до конца дня).</summary>
    public DateTime? DateTo { get; set; }

    /// <summary>Фильтр по вузам. Пустой список = все доступные вузы.</summary>
    public List<int> UniversityIds { get; set; } = new();

    /// <summary>Фильтр по ИТ-направлениям (через program.direction_id).</summary>
    public List<int> DirectionIds { get; set; } = new();

    /// <summary>Фильтр по ИТ-продуктам.</summary>
    public List<int> ProductIds { get; set; } = new();

    // Раньше называлось ResponsibleUserIds — фронт слал managerIds,
    // свойство оставалось пустым и фильтр не применялся.
    /// <summary>Фильтр по ответственным (interactions.manager_id).</summary>
    public List<int> ManagerIds { get; set; } = new();

    /// <summary>
    /// Колонки отчёта — ключи из ALL_COLUMNS фронтенда:
    /// "university", "direction", "product", "status", "manager",
    /// "vendor", "contractNumber".
    /// Бэкенд формирует файл строго по этим колонкам (требование 4 ТЗ).
    /// Пустой список трактуется как «все колонки» — для прямых вызовов API.
    /// </summary>
    public List<string> Columns { get; set; } = new();
}