using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wisdom.Course.Domain.Entities;
namespace Wisdom.Course.Infrastructure.Data.Configurations;

public class CourseTagConfiguration : IEntityTypeConfiguration<CourseTagEntity>
{
    public void Configure(EntityTypeBuilder<CourseTagEntity> builder)
    {
        builder.ToTable("CourseTags");

        builder.HasKey(ct => new { ct.CourseId, ct.TagId });

        builder.Property(ct => ct.CourseId)
            .IsRequired();

        builder.Property(ct => ct.TagId)
            .IsRequired();
    }
}
