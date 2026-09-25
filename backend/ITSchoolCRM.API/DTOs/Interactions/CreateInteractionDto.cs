using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Interactions
{
    public class CreateInteractionDto
    {
        public int? UniversityId { get; set; }

        public int? ProgramId { get; set; }

        public int? ProductId { get; set; }

        public int? ManagerId { get; set; }

        public int? UniversityContactId { get; set; }

        public int? WorkflowId { get; set; }

        public int? ContractId { get; set; }

        public int? LicenseId { get; set; }
    }
}