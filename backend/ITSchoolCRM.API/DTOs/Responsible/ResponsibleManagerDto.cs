using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Responsible
{
    public class ResponsibleManagerDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public int UniversitiesCount { get; set; }
        public List<ResponsibleUniversityDto> Universities { get; set; } = new();
    }
}