using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Contracts
{
    public class ContractInteractionOptionDto
    {
        public int Id { get; set; }
        public string? UniversityName { get; set; }
        public string? ProductName { get; set; }
        public string? ProgramName { get; set; }
    }
}