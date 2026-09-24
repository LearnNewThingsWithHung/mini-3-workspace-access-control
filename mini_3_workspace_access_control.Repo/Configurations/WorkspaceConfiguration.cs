using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using mini_3_workspace_access_control.Repo.Entity;

namespace mini_3_workspace_access_control.Repo.Configurations;

public sealed class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces", table =>
        {
            table.HasCheckConstraint("ck_workspaces_code_normalized", "code = lower(btrim(code))");
            table.HasCheckConstraint("ck_workspaces_code_format", "code ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
            table.HasCheckConstraint("ck_workspaces_name_not_blank", "length(btrim(name)) > 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_workspaces_code");

        builder.HasOne(x => x.CreatedByPerson)
            .WithMany(x => x.CreatedWorkspaces)
            .HasForeignKey(x => x.CreatedByPersonId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_workspaces_people_created_by_person_id");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
