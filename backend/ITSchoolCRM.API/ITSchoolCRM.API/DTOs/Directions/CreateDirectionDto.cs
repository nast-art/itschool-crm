using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Directions
{
    public class CreateDirectionDto
    {
            public string? Name { get; set; }

    public string? Description { get; set; }

        public bool? IsActive { get; set; }

    public int? Priority { get; set; }
    }
}