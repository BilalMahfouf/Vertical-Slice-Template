using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryApi.Domain.Animals;

namespace VeterinaryApi.Infrastructure.Persistence.Configurations.Animals;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {

        builder.HasQueryFilter(a => !a.IsDeleted);
        builder.ToTable("animals");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.ClinicId)
            .HasColumnName("clinic_id")
            .IsRequired();

        builder.HasOne(a => a.Clinic)
            .WithMany() // add navigation collection on Clinic if bidirectional nav is desired
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_animals_clinics_clinic_id");

        builder.Property(a => a.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.HasOne(a => a.Client)
            .WithMany(a => a.Animals) // add navigation collection on Client/Owner if desired
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_animals_owners_client_id");

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Species)
            .HasColumnName("species")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Breed)
            .HasColumnName("breed")
            .HasMaxLength(100);

        builder.Property(a => a.Gender)
            .HasColumnName("gender")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(10)
            .IsRequired();


        builder.Property(a => a.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("date");

        builder.Property(a => a.Color)
            .HasColumnName("color")
            .HasMaxLength(50);

        builder.Property(a => a.MicrochipNumber)
            .HasColumnName("microchip_number")
            .HasMaxLength(50);

        builder.Property(a => a.CreatedOnUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedOnUtc)
            .HasColumnName("updated_at");

        builder.Property(a => a.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(a => a.DeletedOnUtc)
            .HasColumnName("deleted_on_utc");

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("ix_animals_tenant_id");
    }
}
