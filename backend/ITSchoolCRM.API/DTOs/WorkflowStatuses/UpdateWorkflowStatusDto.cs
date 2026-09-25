using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.WorkflowStatuses
{
    public class UpdateWorkflowStatusDto
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsInitial { get; set; }

        public bool? IsFinal { get; set; }
    }
}