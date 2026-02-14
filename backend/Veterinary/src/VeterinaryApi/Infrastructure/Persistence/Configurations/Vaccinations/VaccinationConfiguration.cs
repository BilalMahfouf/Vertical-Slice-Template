using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryApi.Domain.Vaccinations;

namespace VeterinaryApi.Infrastructure.Persistence.Configurations.Vaccinations;

public class VaccinationConfiguration : IEntityTypeConfiguration<Vaccination>
{
    public void Configure(EntityTypeBuilder<Vaccination> builder)
    {
        builder.ToTable("vaccinations");

        // Primary Key
        builder.HasKey(v => v.Id);

        // Base Entity Properties
        builder.Property(v => v.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(v => v.CreatedOnUtc)
            .HasColumnName("created_on_utc")
            .IsRequired();

        builder.Property(v => v.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(v => v.DeletedOnUtc)
            .HasColumnName("deleted_on_utc")
            .IsRequired(false);

        builder.Property(v => v.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .IsRequired();

        // Vaccination Properties

        builder.Property(v => v.AnimalId)
            .HasColumnName("animal_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(v => v.VisitId)
            .HasColumnName("visit_id")
            .HasColumnType("uuid")
            .IsRequired(false);

        builder.Property(v => v.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.GivenAt)
            .HasColumnName("given_at")
            .IsRequired();
        builder.Property(v => v.DueTo)
                   .HasColumnName("due_to")
                   .IsRequired(false);



        builder.Property(v => v.Notes)
            .HasColumnName("notes")
            .IsRequired(false);

        // Foreign Keys
        builder.HasOne(v => v.Animal)
            .WithMany()
            .HasForeignKey(v => v.AnimalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_vaccination_animal");

        builder.HasOne(v => v.Visit)
            .WithMany()
            .HasForeignKey(v => v.VisitId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_vaccination_visit")
            .IsRequired(false);

        // Indexes
        builder.HasIndex(v => v.TenantId)
            .HasDatabaseName("ix_vaccination_tenant_id");

        builder.HasIndex(v => v.IsDeleted)
            .HasDatabaseName("ix_vaccination_is_deleted");

        builder.HasIndex(v => new { v.TenantId, v.IsDeleted })
            .HasDatabaseName("ix_vaccination_tenant_is_deleted");

        builder.HasIndex(v => v.AnimalId)
            .HasDatabaseName("ix_vaccination_animal_id");

        builder.HasIndex(v => v.VisitId)
            .HasDatabaseName("ix_vaccination_visit_id");

        builder.HasIndex(v => v.GivenAt)
            .HasDatabaseName("ix_vaccination_given_at");
    }
}
