using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class interaction_status_history
{
    public int interaction_status_history_id { get; set; }

    public int? interaction_id { get; set; }

    public int? from_status_id { get; set; }

    public int? to_status_id { get; set; }

    public int? changed_by { get; set; }

    public string? comment { get; set; }

    public DateTime? changed_at { get; set; }

    public virtual user? changed_byNavigation { get; set; }

    public virtual workflow_status? from_status { get; set; }

    public virtual interaction? interaction { get; set; }

    public virtual workflow_status? to_status { get; set; }
}
