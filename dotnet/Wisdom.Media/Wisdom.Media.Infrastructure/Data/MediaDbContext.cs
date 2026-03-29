using Microsoft.EntityFrameworkCore;
using Wisdom.Media.Domain.Entities;
using Wisdom.Media.Infrastructure.Data.Configurations;
namespace Wisdom.Media.Infrastructure.Data;

public class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<MediaEntity> Media => Set<MediaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MediaConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
