using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryApi.Domain.Clinics;

namespace VeterinaryApi.Infrastructure.Persistence.Configurations.Clinics;

/// <summary>EF Core fluent configuration for the <see cref="Clinic"/> entity, mapping to the <c>clinics</c> table.</summary>
public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    /// <summary>Configures soft-delete global query filter, table mapping, keys, columns, and relationships.</summary>
    public void Configure(EntityTypeBuilder<Clinic> builder)
    {
        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.ToTable("clinics");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.DoctorId)
            .HasColumnName("doctor_id")
            .IsRequired();

        builder.HasOne(d => d.Doctor)
            .WithOne(d => d.Clinic);

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Address)
            .HasColumnName("address")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.CreatedOnUtc)
            .HasColumnName("created_on_utc");

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(c => c.DeletedOnUtc)
            .HasColumnName("deleted_on_utc");

        builder.Property(c => c.StaffCount)
            .HasColumnName("staff_count");

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_clinics_tenant_id");
    }
}
