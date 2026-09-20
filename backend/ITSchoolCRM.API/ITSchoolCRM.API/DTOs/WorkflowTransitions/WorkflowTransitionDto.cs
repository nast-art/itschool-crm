using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.WorkflowTransitions
{
    public class WorkflowTransitionDto
    {
            public int Id { get; set; }

    public int? WorkflowId { get; set; }

    public int? FromStatusId { get; set; }

    public string? FromStatusName { get; set; }

    public int? ToStatusId { get; set; }

    public string? ToStatusName { get; set; }
    }
}