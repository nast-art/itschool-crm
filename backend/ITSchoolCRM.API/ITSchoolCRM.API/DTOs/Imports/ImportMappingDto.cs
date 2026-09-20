using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Imports
{
    public class ImportMappingDto
    {
           public Dictionary<string, string> Mapping { get; set; } = new();
    }
}