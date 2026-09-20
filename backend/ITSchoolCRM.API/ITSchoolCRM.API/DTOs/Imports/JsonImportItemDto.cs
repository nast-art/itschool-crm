using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Imports
{
    public class JsonImportItemDto
    {
          public int? InteractionId { get; set; }

    public int? UniversityId { get; set; }

    public string? UniversityName { get; set; }

    public int? DirectionId { get; set; }

    public string? DirectionName { get; set; }

    public int? ProgramId { get; set; }

    public string? ProgramName { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductVendor { get; set; }

    public int? ManagerId { get; set; }

    public string? ManagerFullName { get; set; }

    public int? UniversityContactId { get; set; }

    public string? UniversityContactFullName { get; set; }

    public string? UniversityContactPosition { get; set; }

    public string? UniversityContactEmail { get; set; }

    public string? UniversityContactPhone { get; set; }

    public int? WorkflowId { get; set; }

    public string? WorkflowName { get; set; }

    public int? StatusId { get; set; }

    public string? StatusName { get; set; }

    public string? Comment { get; set; }
    }
}