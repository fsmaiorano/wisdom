using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.Domain.ValueObjects;
namespace Wisdom.Order.Infrastructure.Data.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<PurchaseEntity>
{
    public void Configure(EntityTypeBuilder<PurchaseEntity> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(purchaseId => purchaseId.Value, dbId => PurchaseId.Of(dbId));

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.CourseId)
            .IsRequired();

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}
