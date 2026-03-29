using Microsoft.EntityFrameworkCore;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.Infrastructure.Data.Configurations;
namespace Wisdom.Order.Infrastructure.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<PurchaseEntity> Purchases => Set<PurchaseEntity>();
    public DbSet<EnrollmentEntity> Enrollments => Set<EnrollmentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PurchaseConfiguration());
        modelBuilder.ApplyConfiguration(new EnrollmentConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
