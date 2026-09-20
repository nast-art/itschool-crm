using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Universities
{
    public class UpdateUniversityDto
    {
            public string? Name { get; set; }

    public string? ShortName { get; set; }

    public bool? IsActive { get; set; }
    }
}