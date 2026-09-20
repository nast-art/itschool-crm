using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Reports
{
    public class ReportFilterDto
    {
     public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public List<int> UniversityIds { get; set; } = new();

    public List<int> DirectionIds { get; set; } = new();

    public List<int> ProductIds { get; set; } = new();

    public List<int> ResponsibleUserIds { get; set; } = new();

    public bool IncludeUniversity { get; set; } = true;

    public bool IncludeDirection { get; set; } = true;

    public bool IncludeProduct { get; set; } = true;

    public bool IncludeStatus { get; set; } = true;

    public bool IncludeResponsible { get; set; } = true;
    }
}