using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wisdom.User.Domain.Entities;
using Wisdom.User.Domain.Enums;
using Wisdom.User.Domain.ValueObjects;
namespace Wisdom.User.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.KeycloakId).IsUnique();

        builder.Property(u => u.Id)
            .HasConversion(userId => userId.Value, dbUserId => UserId.Of(dbUserId));

        builder.Property(u => u.KeycloakId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.IsActive);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.LastModifiedAt);
    }
}