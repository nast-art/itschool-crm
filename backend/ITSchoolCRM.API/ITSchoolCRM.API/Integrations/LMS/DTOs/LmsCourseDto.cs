using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Integrations.LMS.DTOs
{
     public class LmsCourseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? DirectionName { get; set; }

        public bool IsActive { get; set; }
    }
}