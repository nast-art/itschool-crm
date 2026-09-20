namespace ITSchoolCRM.API.Integrations.Website.DTOs
{
    /// <summary>
    /// Заявка с веб-сайта (B2C-направление: обучение частных лиц,
    /// а также заявки на программы от физических и юридических лиц).
    /// </summary>
    public class WebsiteApplicationDto
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? OrganizationName { get; set; }

        public string? ProgramName { get; set; }

        public string? Status { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}