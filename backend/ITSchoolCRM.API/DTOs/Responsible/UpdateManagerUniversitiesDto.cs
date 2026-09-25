using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Responsible
{
    public class UpdateManagerUniversitiesDto
    {
        public List<int> UniversityIds { get; set; } = new();
    }
}