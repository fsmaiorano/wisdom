using Microsoft.EntityFrameworkCore;
using Wisdom.Learning.Domain.Entities;
using Wisdom.Learning.Infrastructure.Data.Configurations;
namespace Wisdom.Learning.Infrastructure.Data;

public class LearningDbContext(DbContextOptions<LearningDbContext> options) : DbContext(options)
{
    public DbSet<ProgressEntity> Progress => Set<ProgressEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProgressConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
