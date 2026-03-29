using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Infrastructure.Data.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<LessonEntity>
{
    public void Configure(EntityTypeBuilder<LessonEntity> builder)
    {
        builder.ToTable("Lessons");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(lessonId => lessonId.Value, dbId => LessonId.Of(dbId));

        builder.Property(l => l.CourseId)
            .IsRequired()
            .HasConversion(courseId => courseId.Value, dbId => CourseId.Of(dbId));

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(l => l.Description)
            .IsRequired();

        builder.Property(l => l.Position)
            .IsRequired();

        builder.Property(l => l.IsPreview)
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.LastModifiedAt);
    }
}
