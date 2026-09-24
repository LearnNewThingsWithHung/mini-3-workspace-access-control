using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using mini_3_workspace_access_control.Repo.Entity;
using mini_3_workspace_access_control.Repo.Enum;

namespace mini_3_workspace_access_control.Repo.Configurations;

public sealed class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable("workspace_members", table =>
        {
            table.HasCheckConstraint(
                "ck_workspace_members_role_valid",
                "role IN ('Owner', 'Manager', 'Editor', 'Viewer')");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(WorkspaceRole.Viewer)
            .IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();

        builder.HasIndex(x => new { x.WorkspaceId, x.PersonId })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ux_workspace_members_workspace_person_active");
        builder.HasIndex(x => new { x.WorkspaceId, x.Role })
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_workspace_members_workspace_role_active");

        builder.HasOne(x => x.Workspace)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_workspace_members_workspaces_workspace_id");
        builder.HasOne(x => x.Person)
            .WithMany(x => x.WorkspaceMemberships)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_workspace_members_people_person_id");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
