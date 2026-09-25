using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Interactions
{
    public class ChangeInteractionStatusDto
    {
        public int? ToStatusId { get; set; }

        public string? Comment { get; set; }
    }
}