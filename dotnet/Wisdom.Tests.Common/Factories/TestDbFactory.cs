using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Wisdom.Tests.Common.Factories;

public static class TestDbFactory
{

    public static void ReplaceWithInMemory<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;

        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<TContext>) ||
                d.ServiceType == typeof(TContext) ||
                (d.ServiceType.FullName?.Contains(contextName) ?? false) ||
                (d.ServiceType.FullName?.Contains("Npgsql") ?? false))
            .ToList();

        foreach (var descriptor in toRemove)
            services.Remove(descriptor);

        var dbName = $"{contextName}-{Guid.NewGuid():N}";
        services.AddDbContext<TContext>(options => options.UseInMemoryDatabase(dbName));
    }

    public static TContext GetContext<TContext>(IServiceProvider services)
        where TContext : DbContext
        => services.CreateScope().ServiceProvider.GetRequiredService<TContext>();
}
