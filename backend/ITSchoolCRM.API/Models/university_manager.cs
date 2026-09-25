using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class university_manager
{
    public int university_managers_id { get; set; }

    public int? university_id { get; set; }

    public int? user_id { get; set; }

    public DateTime? assigned_at { get; set; }

    public bool? is_primary { get; set; }

    public virtual university? university { get; set; }

    public virtual user? user { get; set; }
}
