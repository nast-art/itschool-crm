using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Imports
{
    public class JsonImportRequestDto
    {
           public List<JsonImportItemDto> Items { get; set; } = new();
    }
}