using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryApi.Domain.Users;

namespace VeterinaryApi.Infrastructure.Persistence.Configurations.Users;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(s => s.Token)
            .HasColumnName("token")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(s => s.TokenType)
            .HasColumnName("token_type")
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(s => s.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(s => s.CreatedOnUtc)
            .HasColumnName("created_on_utc");

        // Index on UserId for faster lookups
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("ix_user_sessions_user_id");

        // Index on Token for faster token validation
        builder.HasIndex(s => s.Token)
            .HasDatabaseName("ix_user_sessions_token");

        // Relationship configured in UserConfiguration (parent side)
        // but can also be defined here if preferred:
        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
