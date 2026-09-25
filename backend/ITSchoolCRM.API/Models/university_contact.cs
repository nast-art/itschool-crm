using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class university_contact
{
    public int university_contacts_id { get; set; }

    public int? university_id { get; set; }

    public string? full_name { get; set; }

    public string? position { get; set; }

    public string? email { get; set; }

    public string? phone { get; set; }

    public bool? is_active { get; set; }

    public string? comment { get; set; }

    public virtual ICollection<interaction> interactions { get; set; } = new List<interaction>();

    public virtual university? university { get; set; }
}
