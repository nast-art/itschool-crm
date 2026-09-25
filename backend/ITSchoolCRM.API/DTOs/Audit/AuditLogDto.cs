using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Audit
{
    public class AuditLogDto
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string? UserName { get; set; }

        public string? Action { get; set; }

        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        public string? OldData { get; set; }

        public string? NewData { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}