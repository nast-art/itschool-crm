using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class attachment
{
    public int attachments_id { get; set; }

    public int? interaction_id { get; set; }

    public int? status_id { get; set; }

    public int? uploaded_by { get; set; }

    public string? file_name { get; set; }

    public string? storage_path { get; set; }

    public string? mime_type { get; set; }

    public long? file_size { get; set; }

    public DateTime? created_at { get; set; }

    public virtual interaction? interaction { get; set; }

    public virtual workflow_status? status { get; set; }

    public virtual user? uploaded_byNavigation { get; set; }
}
