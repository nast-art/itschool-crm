using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Integrations.LMS.DTOs
{
   /// <summary>
    /// Данные взаимодействия CRM, отправляемые во внешнюю LMS
    /// (направление CRM → LMS).
    /// </summary>
    public class PushInteractionToLmsDto
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