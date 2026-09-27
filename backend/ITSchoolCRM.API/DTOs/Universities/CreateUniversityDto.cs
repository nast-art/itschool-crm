using System;

namespace ITSchoolCRM.API.DTOs.Universities
{
    /// <summary>
    /// DTO создания вуза с страницы «Вузы».
    /// Повторяет согласованный маппинг требования 1 ТЗ —
    /// все поля формы «Добавить вуз».
    /// </summary>
    public class CreateUniversityDto
    {
        /// <summary>Название вуза (обязательно, unique — universities_name_key).</summary>
        public string? Name { get; set; }

        /// <summary>Краткое название.</summary>
        public string? ShortName { get; set; }

        /// <summary>Продукт (колонка «ПО» таблицы).</summary>
        public int? ProductId { get; set; }

        /// <summary>Менеджер — ответственный от Школы.</summary>
        public int? ManagerId { get; set; }

        /// <summary>№ договора.</summary>
        public string? ContractNumber { get; set; }

        /// <summary>Дата подписания лицензии.</summary>
        public DateTime? LicenseSignedAt { get; set; }

        /// <summary>Срок действия лицензии — ГОД (например, 2027).</summary>
        public int? LicenseValidYears { get; set; }

        /// <summary>Статус передачи лицензии.</summary>
        public string? TransferStatus { get; set; }

        /// <summary>Ответственные от ВУЗа (контактное лицо).</summary>
        public string? UniversityContactName { get; set; }

        /// <summary>Комментарий.</summary>
        public string? Comment { get; set; }
    }
}