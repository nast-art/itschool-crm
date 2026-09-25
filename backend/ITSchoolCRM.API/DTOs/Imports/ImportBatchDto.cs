using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Imports
{
    public class ImportBatchDto
    {
        public int Id { get; set; }

        public string? FileName { get; set; }

        public int? UploadedBy { get; set; }

        public string? Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? ErrorMessage { get; set; }
    }
}