using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<TagEntity>
{
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.ToTable("Tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(tagId => tagId.Value, dbId => TagId.Of(dbId));

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.LastModifiedAt);
    }
}
