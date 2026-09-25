using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Integrations.LMS.DTOs
{
    public class LmsStudentDto
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? UniversityName { get; set; }

        public string? ProgramName { get; set; }

        public bool IsActive { get; set; }
    }
}