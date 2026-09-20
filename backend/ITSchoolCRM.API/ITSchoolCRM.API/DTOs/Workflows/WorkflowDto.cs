using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITSchoolCRM.API.DTOs;
using ITSchoolCRM.API.DTOs.WorkflowStatuses;
using ITSchoolCRM.API.DTOs.WorkflowTransitions;

namespace ITSchoolCRM.API.DTOs.Workflows
{
    public class WorkflowDto
    {
            public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public int? Version { get; set; }

    public List<WorkflowStatusDto> Statuses { get; set; } = new();

    public List<WorkflowTransitionDto> Transitions { get; set; } = new();
    }
}