using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class it_product
{
    public int it_products_id { get; set; }

    public string? name { get; set; }

    public string? vendor { get; set; }

    public string? description { get; set; }

    public bool? is_active { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<interaction> interactions { get; set; } = new List<interaction>();

    public virtual ICollection<program_product> program_products { get; set; } = new List<program_product>();
}
