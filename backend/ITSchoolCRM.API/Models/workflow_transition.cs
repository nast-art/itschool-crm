using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class workflow_transition
{
    public int workflow_transitions_id { get; set; }

    public int? workflow_id { get; set; }

    public int? from_status_id { get; set; }

    public int? to_status_id { get; set; }

    public virtual workflow_status? from_status { get; set; }

    public virtual workflow_status? to_status { get; set; }

    public virtual workflow? workflow { get; set; }
}
