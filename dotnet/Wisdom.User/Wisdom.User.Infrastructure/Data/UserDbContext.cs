using Microsoft.EntityFrameworkCore;
using Wisdom.User.Domain.Entities;
using Wisdom.User.Domain.Enums;
using Wisdom.User.Domain.ValueObjects;
using Wisdom.User.Infrastructure.Data.Configurations;
namespace Wisdom.User.Infrastructure.Data;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());

        SeedDemoUsers(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void SeedDemoUsers(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<UserEntity>().HasData(
            new
            {
                Id = UserId.Of(new Guid("11111111-0000-0000-0000-000000000001")),
                KeycloakId = "seed-teacher-demo",
                Email = "teacher@wisdom.dev",
                Name = "Teacher Demo",
                Password = "demoteacher",
                Role = UserRoleEnum.Teacher,
                IsActive = true,
                CreatedAt = (DateTime?)seedDate,
                LastModifiedAt = (DateTime?)null,
            },
            new
            {
                Id = UserId.Of(new Guid("22222222-0000-0000-0000-000000000002")),
                KeycloakId = "seed-customer-demo",
                Email = "customer@wisdom.dev",
                Name = "Customer Demo",
                Password = "democustomer",
                Role = UserRoleEnum.Customer,
                IsActive = true,
                CreatedAt = (DateTime?)seedDate,
                LastModifiedAt = (DateTime?)null,
            }
        );
    }
}
