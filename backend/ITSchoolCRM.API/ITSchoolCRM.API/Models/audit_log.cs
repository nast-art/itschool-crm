using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class audit_log
{
    public int audit_logs_id { get; set; }

    public int? user_id { get; set; }

    public string? action { get; set; }

    public string? entity_type { get; set; }

    public int? entity_id { get; set; }

    public string? old_data { get; set; }

    public string? new_data { get; set; }

    public DateTime? created_at { get; set; }

    public virtual user? user { get; set; }
}
