using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Attachments
{
    public class AttachmentDto
    {
        public int Id { get; set; }

        public int? InteractionId { get; set; }

        public int? StatusId { get; set; }
        public string? StatusName { get; set; }

        public int? UploadedBy { get; set; }

        public string? FileName { get; set; }

        public string? MimeType { get; set; }

        public long? FileSize { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}