using System;
using System.Collections.Generic;

namespace ITSchoolCRM.API.Models;

public partial class interaction
{
    public int interactions_id { get; set; }

    public int? university_id { get; set; }

    public int? program_id { get; set; }

    public int? product_id { get; set; }

    public int? manager_id { get; set; }

    public int? university_contact_id { get; set; }

    public int? workflow_id { get; set; }

    public int? current_status_id { get; set; }

    public int? contract_id { get; set; }

    public int? license_id { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<attachment> attachments { get; set; } = new List<attachment>();

    public virtual contract? contract { get; set; }

    public virtual workflow_status? current_status { get; set; }

    public virtual ICollection<interaction_status_history> interaction_status_histories { get; set; } = new List<interaction_status_history>();

    public virtual license? license { get; set; }

    public virtual user? manager { get; set; }

    public virtual it_product? product { get; set; }

    public virtual it_program? program { get; set; }

    public virtual university? university { get; set; }

    public virtual university_contact? university_contact { get; set; }

    public virtual workflow? workflow { get; set; }
}
