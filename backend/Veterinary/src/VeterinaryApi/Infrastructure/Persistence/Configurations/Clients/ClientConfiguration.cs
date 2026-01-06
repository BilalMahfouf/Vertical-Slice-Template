using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryApi.Domain.Clients;

namespace VeterinaryApi.Infrastructure.Persistence.Configurations.Clients;


public class OwnerConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("owners");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(o => o.ClinicId)
            .HasColumnName("clinic_id")
            .IsRequired();

        builder.HasOne(o => o.Clinic)
            .WithMany() // Clinic does not currently expose Owners collection
            .HasForeignKey(o => o.ClinicId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_owners_clinics_clinic_id");

        builder.Property(o => o.FullName)
          .HasColumnName("full_name")
          .HasMaxLength(50)
          .IsRequired();

        builder.Property(o => o.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.Notes)
            .HasColumnName("notes");

        builder.Property(o => o.CreatedOnUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedOnUtc)
            .HasColumnName("updated_at");

        // Soft-delete columns (Entity provides these properties)
        builder.Property(o => o.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(o => o.DeletedOnUtc)
            .HasColumnName("deleted_on_utc");
    }
}
