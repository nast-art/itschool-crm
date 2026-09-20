using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class university
{
    public int universities_id { get; set; }

    public string? name { get; set; }

    public string? short_name { get; set; }

    public bool? is_active { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<interaction> interactions { get; set; } = new List<interaction>();

    public virtual ICollection<university_contact> university_contacts { get; set; } = new List<university_contact>();

    public virtual ICollection<university_manager> university_managers { get; set; } = new List<university_manager>();
}
