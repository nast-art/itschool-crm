using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Interactions
{
    public class InteractionDto
    {
        public int Id { get; set; }

        public int? UniversityId { get; set; }

        public string? UniversityName { get; set; }

        public int? ProgramId { get; set; }

        public string? ProgramName { get; set; }

        public int? ProductId { get; set; }

        public string? ProductName { get; set; }

        public int? ManagerId { get; set; }

        public string? ManagerName { get; set; }

        public int? UniversityContactId { get; set; }

        public string? UniversityContactName { get; set; }

        public int? WorkflowId { get; set; }

        public string? WorkflowName { get; set; }

        public int? CurrentStatusId { get; set; }

        public string? CurrentStatusName { get; set; }

        public int? ContractId { get; set; }

        public int? LicenseId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}