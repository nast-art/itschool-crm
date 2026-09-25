using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class it_direction
{
    public int it_directions_id { get; set; }

    public string? name { get; set; }

    public string? description { get; set; }

    public bool? is_active { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public int? priority { get; set; }

    public virtual ICollection<it_program> it_programs { get; set; } = new List<it_program>();
}
