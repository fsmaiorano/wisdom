using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wisdom.Learning.Domain.Entities;
using Wisdom.Learning.Domain.ValueObjects;
namespace Wisdom.Learning.Infrastructure.Data.Configurations;

public class ProgressConfiguration : IEntityTypeConfiguration<ProgressEntity>
{
    public void Configure(EntityTypeBuilder<ProgressEntity> builder)
    {
        builder.ToTable("Progress");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(progressId => progressId.Value, v => ProgressId.Of(v));

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.LessonId)
            .IsRequired();

        builder.Property(p => p.WatchedSeconds)
            .IsRequired();

        builder.Property(p => p.IsCompleted)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.LastModifiedAt);

        builder.HasIndex(p => new { p.UserId, p.LessonId })
            .IsUnique();
    }
}
