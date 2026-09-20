using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class workflow
{
    public int workflows_id { get; set; }

    public string? name { get; set; }

    public string? description { get; set; }

    public bool? is_active { get; set; }

    public int? version { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<interaction> interactions { get; set; } = new List<interaction>();

    public virtual ICollection<workflow_status> workflow_statuses { get; set; } = new List<workflow_status>();

    public virtual ICollection<workflow_transition> workflow_transitions { get; set; } = new List<workflow_transition>();
}
