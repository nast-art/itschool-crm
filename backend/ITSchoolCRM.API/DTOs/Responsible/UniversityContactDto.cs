using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Responsible
{
    public class UniversityContactDto
    {
        public int Id { get; set; }
        public int UniversityId { get; set; }
        public string? UniversityName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public string? Comment { get; set; }
    }
}