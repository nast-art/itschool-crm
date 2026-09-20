using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.WorkflowTransitions
{
    public class CreateWorkflowTransitionDto
    {
           public int? WorkflowId { get; set; }

    public int? FromStatusId { get; set; }

    public int? ToStatusId { get; set; }
    }
}