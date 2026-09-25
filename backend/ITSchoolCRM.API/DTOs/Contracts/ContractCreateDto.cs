using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Contracts
{
    public class ContractCreateDto
    {
        public string? ContractNumber { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string? Status { get; set; }
        public string? Comment { get; set; }
        public int? InteractionId { get; set; }
    }
}