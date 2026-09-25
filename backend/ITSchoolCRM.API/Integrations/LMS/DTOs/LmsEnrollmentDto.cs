using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Integrations.LMS.DTOs
{
    public class LmsEnrollmentDto
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public string? StudentFullName { get; set; }

        public int CourseId { get; set; }

        public string? CourseName { get; set; }

        public string? Status { get; set; }

        public int ProgressPercent { get; set; }

        public DateTime? EnrolledAt { get; set; }
    }
}