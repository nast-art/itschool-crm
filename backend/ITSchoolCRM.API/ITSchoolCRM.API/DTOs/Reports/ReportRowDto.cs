using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Reports
{
    public class ReportRowDto
    {
           public int InteractionId { get; set; }

    public string? UniversityName { get; set; }

    public string? DirectionName { get; set; }

    public string? ProductName { get; set; }

    public string? StatusName { get; set; }

    public string? ResponsibleName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    }
}