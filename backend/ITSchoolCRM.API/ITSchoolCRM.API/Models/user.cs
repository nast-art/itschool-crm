using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class user
{
    public int users_id { get; set; }

    public string? keycloak_user_id { get; set; }

    public string? email { get; set; }

    public bool? is_active { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public string? last_name { get; set; }

    public string? first_name { get; set; }

    public string? middle_name { get; set; }

    public virtual ICollection<attachment> attachments { get; set; } = new List<attachment>();

    public virtual ICollection<audit_log> audit_logs { get; set; } = new List<audit_log>();

    public virtual ICollection<import_batch> import_batches { get; set; } = new List<import_batch>();

    public virtual ICollection<interaction_status_history> interaction_status_histories { get; set; } = new List<interaction_status_history>();

    public virtual ICollection<interaction> interactions { get; set; } = new List<interaction>();

    public virtual ICollection<university_manager> university_managers { get; set; } = new List<university_manager>();
}
