using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryApi.Domain.Notifications;

namespace VeterinaryApi.Infrastructure.Persistence.Configurations.Notifications;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasQueryFilter(n => !n.IsDeleted);

        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasColumnName("body")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasColumnName("is_read")
            .IsRequired();

        builder.Property(n => n.CreatedOnUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(n => n.DeletedOnUtc)
            .HasColumnName("deleted_at");
    }
}
