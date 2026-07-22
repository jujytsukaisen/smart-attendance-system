using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAttendance.Infrastructure.Identity.Models;

namespace SmartAttendance.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.ReplacedByToken)
            .HasMaxLength(256);

        builder.HasIndex(t => t.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

