namespace ITSchoolCRM.API.DTOs.Reports;

/// <summary>
/// Строка отчёта по взаимодействию. Содержит значения ВСЕХ колонок;
/// какие из них попадут в файл — решает список Columns в фильтре.
/// Источники данных — согласно схеме БД из ТЗ:
///   university  → universities.name
///   direction   → it_programs → it_directions.name
///   product     → it_products.name
///   status      → workflow_statuses.name
///   manager     → users.last_name/first_name/middle_name (full_name удалён миграцией)
///   vendor      → it_products.vendor
///   contract    → contracts.contract_number (через interactions.contract_id)
/// </summary>
public class ReportRowDto
{
    public int InteractionId { get; set; }

    public string? UniversityName { get; set; }

    public string? DirectionName { get; set; }

    public string? ProductName { get; set; }

    public string? StatusName { get; set; }

    /// <summary>ФИО собирается из частей — отдельной колонки full_name в БД нет.</summary>
    public string? ResponsibleName { get; set; }

    /// <summary>Колонка «Вендор» — it_products.vendor.</summary>
    public string? VendorName { get; set; }

    /// <summary>Колонка «№ Договора» — contracts.contract_number.</summary>
    public string? ContractNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}