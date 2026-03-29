using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<CourseEntity>
{
    public void Configure(EntityTypeBuilder<CourseEntity> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(courseId => courseId.Value, dbId => CourseId.Of(dbId));

        builder.Property(c => c.TeacherId)
            .IsRequired();

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.Description)
            .IsRequired();

        builder.Property(c => c.Price)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(c => c.IsPublished)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.LastModifiedAt);
    }
}
