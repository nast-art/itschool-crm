namespace ITSchoolCRM.API.Integrations.Website.DTOs
{
    /// <summary>
    /// Данные взаимодействия CRM, отправляемые на веб-сайт
    /// (направление CRM → сайт).
    /// </summary>
    public class PushInteractionToWebsiteDto
    {
        public int InteractionId { get; set; }

        public string? UniversityName { get; set; }

        public string? ProgramName { get; set; }

        public string? ProductName { get; set; }

        public string? StatusName { get; set; }

        public string? ManagerName { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}