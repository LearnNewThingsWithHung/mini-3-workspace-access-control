using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using mini_3_workspace_access_control.Repo.Configurations.Seed;
using mini_3_workspace_access_control.Repo.Entity;

namespace mini_3_workspace_access_control.Repo.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("people", table =>
        {
            table.HasCheckConstraint("ck_people_email_normalized", "email = lower(btrim(email))");
            table.HasCheckConstraint("ck_people_email_not_blank", "length(btrim(email)) > 0");
            table.HasCheckConstraint("ck_people_display_name_not_blank", "length(btrim(display_name)) > 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();

        builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("ux_people_email");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasData(PersonSeed.Data);
    }
}
