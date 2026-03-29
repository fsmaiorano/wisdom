using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wisdom.Media.Domain.Entities;
using Wisdom.Media.Domain.ValueObjects;
namespace Wisdom.Media.Infrastructure.Data.Configurations;

public class MediaConfiguration : IEntityTypeConfiguration<MediaEntity>
{
    public void Configure(EntityTypeBuilder<MediaEntity> builder)
    {
        builder.ToTable("Media");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(mediaId => mediaId.Value, dbId => MediaId.Of(dbId));

        builder.Property(m => m.LessonId)
            .IsRequired();

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.ThumbnailPath)
            .HasMaxLength(1000);

        builder.Property(m => m.Duration);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.LastModifiedAt);

        builder.HasIndex(m => m.LessonId)
            .IsUnique()
            .HasDatabaseName("IX_Media_LessonId");
    }
}
