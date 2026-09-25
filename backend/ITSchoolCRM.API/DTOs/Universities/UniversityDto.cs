using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Universities
{
    public class UniversityDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? ShortName { get; set; }

        public bool? IsActive { get; set; }
    }
}