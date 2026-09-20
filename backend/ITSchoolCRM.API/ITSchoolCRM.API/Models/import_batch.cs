using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class import_batch
{
    public int import_batches_id { get; set; }

    public string? file_name { get; set; }

    public int? uploaded_by { get; set; }

    public string? status { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? completed_at { get; set; }

    public string? error_message { get; set; }

    public virtual user? uploaded_byNavigation { get; set; }
}
