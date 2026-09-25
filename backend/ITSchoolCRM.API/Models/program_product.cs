using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class program_product
{
    public int program_products_id { get; set; }

    public int? program_id { get; set; }

    public int? product_id { get; set; }

    public virtual it_product? product { get; set; }

    public virtual it_program? program { get; set; }
}
