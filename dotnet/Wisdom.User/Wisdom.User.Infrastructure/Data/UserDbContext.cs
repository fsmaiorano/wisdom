using Microsoft.EntityFrameworkCore;
using Wisdom.User.Domain.Entities;
using Wisdom.User.Infrastructure.Data.Configurations;
namespace Wisdom.User.Infrastructure.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}