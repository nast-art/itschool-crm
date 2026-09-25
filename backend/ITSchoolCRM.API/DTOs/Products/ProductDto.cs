using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Vendor { get; set; }

        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}