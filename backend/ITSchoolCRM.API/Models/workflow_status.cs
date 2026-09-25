using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class workflow_status
{
    public int workflow_statuses_id { get; set; }

    public int? workflow_id { get; set; }

    public string? name { get; set; }

    public string? description { get; set; }

    public int? sort_order { get; set; }

    public bool? is_initial { get; set; }

    public bool? is_final { get; set; }

    public virtual ICollection<attachment> attachments { get; set; } = new List<attachment>();

    public virtual ICollection<interaction_status_history> interaction_status_historyfrom_statuses { get; set; } = new List<interaction_status_history>();

    public virtual ICollection<interaction_status_history> interaction_status_historyto_statuses { get; set; } = new List<interaction_status_history>();

    public virtual ICollection<interaction> interactions { get; set; } = new List<interaction>();

    public virtual workflow? workflow { get; set; }

    public virtual ICollection<workflow_transition> workflow_transitionfrom_statuses { get; set; } = new List<workflow_transition>();

    public virtual ICollection<workflow_transition> workflow_transitionto_statuses { get; set; } = new List<workflow_transition>();
}
