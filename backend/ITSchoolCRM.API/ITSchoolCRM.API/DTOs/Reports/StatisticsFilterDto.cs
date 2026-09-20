namespace ITSchoolCRM.API.DTOs.Reports;

// Фильтр статистики и отчётов: период + мультивыбор вузов/направлений/продуктов.
// Пустые списки = «все». Доступ по вузам применяется сервисом ОБЯЗАТЕЛЬНО
// (пересечение с university_managers), фронт этому не доверяет.
public class StatisticsFilterDto
{
    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public List<int>? UniversityIds { get; set; }

    public List<int>? DirectionIds { get; set; }

    public List<int>? ProductIds { get; set; }
}