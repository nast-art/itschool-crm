using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.WorkflowTransitions
{
    public class UpdateWorkflowTransitionDto
    {
        public int FromStatusId { get; set; }

        public int ToStatusId { get; set; }
    }
}