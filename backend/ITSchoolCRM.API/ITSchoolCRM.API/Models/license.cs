using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class license
{
    public int licenses_id { get; set; }

    public DateTime? signed_at { get; set; }

    public DateTime? valid_until { get; set; }

    public string? transfer_status { get; set; }

    public string? comment { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<interaction> interactions { get; set; } = new List<interaction>();
}
