using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Reports
{
    public class StatisticsItemDto
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}