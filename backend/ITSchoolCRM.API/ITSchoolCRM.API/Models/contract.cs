using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class contract
{
    public int contracts_id { get; set; }

    public string? contract_number { get; set; }

    public DateTime? signed_at { get; set; }

    public string? status { get; set; }

    public string? comment { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<interaction> interactions { get; set; } = new List<interaction>();
}
