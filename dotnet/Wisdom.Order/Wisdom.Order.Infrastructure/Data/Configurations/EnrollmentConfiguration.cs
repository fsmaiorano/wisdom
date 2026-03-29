using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.Domain.ValueObjects;
namespace Wisdom.Order.Infrastructure.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<EnrollmentEntity>
{
    public void Configure(EntityTypeBuilder<EnrollmentEntity> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(enrollmentId => enrollmentId.Value, dbId => EnrollmentId.Of(dbId));

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.CourseId)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();
    }
}
