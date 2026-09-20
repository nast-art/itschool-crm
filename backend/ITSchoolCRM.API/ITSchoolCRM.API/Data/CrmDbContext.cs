using System;
using System.Collections.Generic;
using ITSchoolCRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Data;

public partial class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<attachment> attachments { get; set; }

    public virtual DbSet<audit_log> audit_logs { get; set; }

    public virtual DbSet<contract> contracts { get; set; }

    public virtual DbSet<import_batch> import_batches { get; set; }

    public virtual DbSet<interaction> interactions { get; set; }

    public virtual DbSet<interaction_status_history> interaction_status_histories { get; set; }

    public virtual DbSet<it_direction> it_directions { get; set; }

    public virtual DbSet<it_product> it_products { get; set; }

    public virtual DbSet<it_program> it_programs { get; set; }

    public virtual DbSet<license> licenses { get; set; }

    public virtual DbSet<program_product> program_products { get; set; }

    public virtual DbSet<university> universities { get; set; }

    public virtual DbSet<university_contact> university_contacts { get; set; }

    public virtual DbSet<university_manager> university_managers { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<workflow> workflows { get; set; }

    public virtual DbSet<workflow_status> workflow_statuses { get; set; }

    public virtual DbSet<workflow_transition> workflow_transitions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<attachment>(entity =>
        {
            entity.HasKey(e => e.attachments_id).HasName("attachments_pkey");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.file_name).HasMaxLength(300);
            entity.Property(e => e.mime_type).HasMaxLength(100);
            entity.Property(e => e.storage_path).HasMaxLength(500);

            entity.HasOne(d => d.interaction).WithMany(p => p.attachments)
                .HasForeignKey(d => d.interaction_id)
                .HasConstraintName("attachments_interaction_id_fkey");

            entity.HasOne(d => d.status).WithMany(p => p.attachments)
                .HasForeignKey(d => d.status_id)
                .HasConstraintName("attachments_status_id_fkey");

            entity.HasOne(d => d.uploaded_byNavigation).WithMany(p => p.attachments)
                .HasForeignKey(d => d.uploaded_by)
                .HasConstraintName("attachments_uploaded_by_fkey");
        });

        modelBuilder.Entity<audit_log>(entity =>
        {
            entity.HasKey(e => e.audit_logs_id).HasName("audit_logs_pkey");

            entity.Property(e => e.action).HasMaxLength(100);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.entity_type).HasMaxLength(100);
            entity.Property(e => e.new_data).HasColumnType("jsonb");
            entity.Property(e => e.old_data).HasColumnType("jsonb");

            entity.HasOne(d => d.user).WithMany(p => p.audit_logs)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("audit_logs_user_id_fkey");
        });

        modelBuilder.Entity<contract>(entity =>
        {
            entity.HasKey(e => e.contracts_id).HasName("contracts_pkey");

            entity.HasIndex(e => e.contract_number, "contracts_contract_number_key").IsUnique();

            entity.Property(e => e.contract_number).HasMaxLength(100);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.signed_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.status).HasMaxLength(100);
        });

        modelBuilder.Entity<import_batch>(entity =>
        {
            entity.HasKey(e => e.import_batches_id).HasName("import_batches_pkey");

            entity.Property(e => e.completed_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.file_name).HasMaxLength(300);
            entity.Property(e => e.status).HasMaxLength(100);

            entity.HasOne(d => d.uploaded_byNavigation).WithMany(p => p.import_batches)
                .HasForeignKey(d => d.uploaded_by)
                .HasConstraintName("import_batches_uploaded_by_fkey");
        });

        modelBuilder.Entity<interaction>(entity =>
        {
            entity.HasKey(e => e.interactions_id).HasName("interactions_pkey");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.updated_at).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.contract).WithMany(p => p.interactions)
                .HasForeignKey(d => d.contract_id)
                .HasConstraintName("interactions_contract_id_fkey");

            entity.HasOne(d => d.current_status).WithMany(p => p.interactions)
                .HasForeignKey(d => d.current_status_id)
                .HasConstraintName("interactions_current_status_id_fkey");

            entity.HasOne(d => d.license).WithMany(p => p.interactions)
                .HasForeignKey(d => d.license_id)
                .HasConstraintName("interactions_license_id_fkey");

            entity.HasOne(d => d.manager).WithMany(p => p.interactions)
                .HasForeignKey(d => d.manager_id)
                .HasConstraintName("interactions_manager_id_fkey");

            entity.HasOne(d => d.product).WithMany(p => p.interactions)
                .HasForeignKey(d => d.product_id)
                .HasConstraintName("interactions_product_id_fkey");

            entity.HasOne(d => d.program).WithMany(p => p.interactions)
                .HasForeignKey(d => d.program_id)
                .HasConstraintName("interactions_program_id_fkey");

            entity.HasOne(d => d.university_contact).WithMany(p => p.interactions)
                .HasForeignKey(d => d.university_contact_id)
                .HasConstraintName("interactions_university_contact_id_fkey");

            entity.HasOne(d => d.university).WithMany(p => p.interactions)
                .HasForeignKey(d => d.university_id)
                .HasConstraintName("interactions_university_id_fkey");

            entity.HasOne(d => d.workflow).WithMany(p => p.interactions)
                .HasForeignKey(d => d.workflow_id)
                .HasConstraintName("interactions_workflow_id_fkey");
        });

        modelBuilder.Entity<interaction_status_history>(entity =>
        {
            entity.HasKey(e => e.interaction_status_history_id).HasName("interaction_status_history_pkey");

            entity.ToTable("interaction_status_history");

            entity.Property(e => e.changed_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.changed_byNavigation).WithMany(p => p.interaction_status_histories)
                .HasForeignKey(d => d.changed_by)
                .HasConstraintName("interaction_status_history_changed_by_fkey");

            entity.HasOne(d => d.from_status).WithMany(p => p.interaction_status_historyfrom_statuses)
                .HasForeignKey(d => d.from_status_id)
                .HasConstraintName("interaction_status_history_from_status_id_fkey");

            entity.HasOne(d => d.interaction).WithMany(p => p.interaction_status_histories)
                .HasForeignKey(d => d.interaction_id)
                .HasConstraintName("interaction_status_history_interaction_id_fkey");

            entity.HasOne(d => d.to_status).WithMany(p => p.interaction_status_historyto_statuses)
                .HasForeignKey(d => d.to_status_id)
                .HasConstraintName("interaction_status_history_to_status_id_fkey");
        });

        modelBuilder.Entity<it_direction>(entity =>
        {
            entity.HasKey(e => e.it_directions_id).HasName("it_directions_pkey");

            entity.HasIndex(e => e.name, "it_directions_name_key").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.name).HasMaxLength(200);
            entity.Property(e => e.updated_at).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<it_product>(entity =>
        {
            entity.HasKey(e => e.it_products_id).HasName("it_products_pkey");

            entity.HasIndex(e => e.name, "it_products_name_key").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.name).HasMaxLength(200);
            entity.Property(e => e.updated_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.vendor).HasMaxLength(200);
        });

        modelBuilder.Entity<it_program>(entity =>
        {
            entity.HasKey(e => e.it_programs_id).HasName("it_programs_pkey");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.name).HasMaxLength(200);
            entity.Property(e => e.updated_at).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.direction).WithMany(p => p.it_programs)
                .HasForeignKey(d => d.direction_id)
                .HasConstraintName("it_programs_direction_id_fkey");
        });

        modelBuilder.Entity<license>(entity =>
        {
            entity.HasKey(e => e.licenses_id).HasName("licenses_pkey");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.signed_at).HasColumnType("timestamp without time zone");
            entity.Property(e => e.transfer_status).HasMaxLength(100);
            entity.Property(e => e.valid_until).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<program_product>(entity =>
        {
            entity.HasKey(e => e.program_products_id).HasName("program_products_pkey");

            entity.HasOne(d => d.product).WithMany(p => p.program_products)
                .HasForeignKey(d => d.product_id)
                .HasConstraintName("program_products_product_id_fkey");

            entity.HasOne(d => d.program).WithMany(p => p.program_products)
                .HasForeignKey(d => d.program_id)
                .HasConstraintName("program_products_program_id_fkey");
        });

        modelBuilder.Entity<university>(entity =>
        {
            entity.HasKey(e => e.universities_id).HasName("universities_pkey");

            entity.HasIndex(e => e.name, "universities_name_key").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.name).HasMaxLength(300);
            entity.Property(e => e.short_name).HasMaxLength(100);
            entity.Property(e => e.updated_at).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<university_contact>(entity =>
        {
            entity.HasKey(e => e.university_contacts_id).HasName("university_contacts_pkey");

            entity.Property(e => e.email).HasMaxLength(200);
            entity.Property(e => e.full_name).HasMaxLength(200);
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.phone).HasMaxLength(50);
            entity.Property(e => e.position).HasMaxLength(200);

            entity.HasOne(d => d.university).WithMany(p => p.university_contacts)
                .HasForeignKey(d => d.university_id)
                .HasConstraintName("university_contacts_university_id_fkey");
        });

        modelBuilder.Entity<university_manager>(entity =>
        {
            entity.HasKey(e => e.university_managers_id).HasName("university_managers_pkey");

            entity.Property(e => e.assigned_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_primary).HasDefaultValue(false);

            entity.HasOne(d => d.university).WithMany(p => p.university_managers)
                .HasForeignKey(d => d.university_id)
                .HasConstraintName("university_managers_university_id_fkey");

            entity.HasOne(d => d.user).WithMany(p => p.university_managers)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("university_managers_user_id_fkey");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.users_id).HasName("users_pkey");

            entity.HasIndex(e => e.email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.keycloak_user_id, "users_keycloak_user_id_key").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.email).HasMaxLength(200);
            entity.Property(e => e.first_name).HasMaxLength(100);
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.keycloak_user_id).HasMaxLength(200);
            entity.Property(e => e.last_name).HasMaxLength(100);
            entity.Property(e => e.middle_name).HasMaxLength(100);
            entity.Property(e => e.updated_at).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<workflow>(entity =>
        {
            entity.HasKey(e => e.workflows_id).HasName("workflows_pkey");

            entity.HasIndex(e => e.name, "workflows_name_key").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.name).HasMaxLength(200);
            entity.Property(e => e.updated_at).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<workflow_status>(entity =>
        {
            entity.HasKey(e => e.workflow_statuses_id).HasName("workflow_statuses_pkey");

            entity.Property(e => e.is_final).HasDefaultValue(false);
            entity.Property(e => e.is_initial).HasDefaultValue(false);
            entity.Property(e => e.name).HasMaxLength(200);

            entity.HasOne(d => d.workflow).WithMany(p => p.workflow_statuses)
                .HasForeignKey(d => d.workflow_id)
                .HasConstraintName("workflow_statuses_workflow_id_fkey");
        });

        modelBuilder.Entity<workflow_transition>(entity =>
        {
            entity.HasKey(e => e.workflow_transitions_id).HasName("workflow_transitions_pkey");

            entity.HasOne(d => d.from_status).WithMany(p => p.workflow_transitionfrom_statuses)
                .HasForeignKey(d => d.from_status_id)
                .HasConstraintName("workflow_transitions_from_status_id_fkey");

            entity.HasOne(d => d.to_status).WithMany(p => p.workflow_transitionto_statuses)
                .HasForeignKey(d => d.to_status_id)
                .HasConstraintName("workflow_transitions_to_status_id_fkey");

            entity.HasOne(d => d.workflow).WithMany(p => p.workflow_transitions)
                .HasForeignKey(d => d.workflow_id)
                .HasConstraintName("workflow_transitions_workflow_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
