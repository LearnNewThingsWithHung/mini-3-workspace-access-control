using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using mini_3_workspace_access_control.Repo.Entity;
using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Repo.Configurations;

public sealed class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
{
    public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
    {
        builder.ToTable("workspace_invitations", table =>
        {
            table.HasCheckConstraint("ck_workspace_invitations_email_normalized", "email = lower(btrim(email))");
            table.HasCheckConstraint("ck_workspace_invitations_email_not_blank", "length(btrim(email)) > 0");
            table.HasCheckConstraint("ck_workspace_invitations_role_valid", "role IN ('Owner', 'Manager', 'Editor', 'Viewer')");
            table.HasCheckConstraint("ck_workspace_invitations_token_hash_hex", "token_hash ~ '^[0-9a-f]{64}$'");
            table.HasCheckConstraint("ck_workspace_invitations_expiry", "expires_at > created_at");
            table.HasCheckConstraint("ck_workspace_invitations_accepted_after_create", "accepted_at IS NULL OR accepted_at >= created_at");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(WorkspaceRole.Viewer)
            .IsRequired();
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
        
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(InvitationStatus.Pending)
            .IsRequired();
        
        builder.HasIndex(x => x.TokenHash).IsUnique().HasDatabaseName("ux_workspace_invitations_token_hash");
        builder.HasIndex(x => new { x.WorkspaceId, x.Email })
            .IsUnique()
            .HasFilter("status = 'Pending' AND is_deleted = false")
            .HasDatabaseName("ux_workspace_invitations_active_workspace_email");
        
        builder.HasIndex(x => x.ExpiresAt)
            .HasFilter("status = 'Expired' AND is_deleted = false")
            .HasDatabaseName("ix_workspace_invitations_pending_expires_at");

        builder.HasOne(x => x.Workspace)
            .WithMany(x => x.Invitations)
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_workspace_invitations_workspaces_workspace_id");
        builder.HasOne(x => x.CreatedByPerson)
            .WithMany(x => x.CreatedWorkspaceInvitations)
            .HasForeignKey(x => x.CreatedByPersonId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_workspace_invitations_people_created_by_person_id");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
