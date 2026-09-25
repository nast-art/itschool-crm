using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Reports
{
    public class StatisticsDto
    {
        public int TotalInteractions { get; set; }

        public List<StatisticsItemDto> ByStatus { get; set; } = new();

        public List<StatisticsItemDto> ByUniversity { get; set; } = new();

        public List<StatisticsItemDto> ByDirection { get; set; } = new();

        public List<StatisticsItemDto> ByProduct { get; set; } = new();
    }
}