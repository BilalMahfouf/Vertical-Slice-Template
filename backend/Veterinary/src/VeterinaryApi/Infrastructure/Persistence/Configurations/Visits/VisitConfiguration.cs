using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryApi.Domain.Visits;

namespace VeterinaryApi.Infrastructure.Persistence.Configurations.Visits;

/// <summary>EF Core fluent configuration for the <see cref="Visit"/> entity, mapping to the <c>visits</c> table.</summary>
public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    /// <summary>Configures soft-delete global query filter, table mapping, primary key, column mappings, and navigation relationships.</summary>
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.HasQueryFilter(v => !v.IsDeleted);

        builder.ToTable("visits");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(v => v.AnimalId)
            .HasColumnName("animal_id")
            .IsRequired();

        builder.HasOne(v => v.Animal)
            .WithMany()
            .HasForeignKey(v => v.AnimalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_visits_animals_animal_id");

        builder.Property(v => v.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(v => v.PaymentAmount)
                   .HasColumnName("payment_amount")
                   .IsRequired();

        builder.Property(v => v.PaymentStatus)
                   .HasColumnName("payment_status")
                   .IsRequired();



        builder.HasOne(v => v.Owner)
            .WithMany(v => v.Visits)
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_visits_owners_owner_id");

        builder.Property(v => v.AppointmentId)
            .HasColumnName("appointment_id")
            .IsRequired(false);

        builder.HasOne(v => v.Appointment)
            .WithMany()
            .HasForeignKey(v => v.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_visits_appointments_appointment_id");

        builder.Property(v => v.VisitType)
            .HasColumnName("visit_type")
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(v => v.Symptoms)
            .HasColumnName("symptoms")
            .HasColumnType("text[]");

        builder.Property(v => v.Diagnosis)
            .HasColumnName("diagnosis")
            .HasColumnType("text[]");

        builder.Property(v => v.Treatment)
            .HasColumnName("treatment")
            .HasColumnType("text[]");

        builder.Property(v => v.Notes)
            .HasColumnName("notes")
            .HasColumnType("text");

        builder.Property(v => v.CreatedOnUtc)
            .HasColumnName("created_on_utc")
            .IsRequired();

        builder.Property(v => v.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(v => v.DeletedOnUtc)
            .HasColumnName("deleted_on_utc");

        builder.Property(v => v.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(v => v.TenantId)
            .HasDatabaseName("ix_visits_tenant_id");
    }
}
