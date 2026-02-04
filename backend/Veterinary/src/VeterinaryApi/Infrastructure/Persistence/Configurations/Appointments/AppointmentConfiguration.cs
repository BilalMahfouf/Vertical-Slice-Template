using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryApi.Domain.Appointments;

namespace VeterinaryApi.Infrastructure.Persistence.Configurations.Appointments;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.ToTable("appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.ClinicId)
            .HasColumnName("clinic_id")
            .IsRequired();

        builder.HasOne(a => a.Clinic)
            .WithMany()
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_appointments_clinics_clinic_id");

        builder.Property(a => a.AnimalId)
            .HasColumnName("animal_id")
            .IsRequired();

        builder.HasOne(a => a.Animal)
            .WithMany()
            .HasForeignKey(a => a.AnimalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_appointments_animals_animal_id");

        builder.Property(a => a.AppointmentDate)
            .HasColumnName("appointment_date")
            .IsRequired();


        builder.Property(a => a.Location)
            .HasColumnName("location")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion<byte>()
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(a => a.StatusUpdatedOnUtc)
            .HasColumnName("status_updated_on_utc");

        builder.Property(a => a.Notes)
            .HasColumnName("notes");

        builder.Property(a => a.CreatedOnUtc)
            .HasColumnName("created_on_utc")
            .IsRequired();

        builder.Property(a => a.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(a => a.DeletedOnUtc)
            .HasColumnName("deleted_on_utc");

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("ix_appointments_tenant_id");
    }
}
