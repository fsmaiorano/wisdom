using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wisdom.Order.Domain.Repositories;
using Wisdom.Order.Infrastructure.Data;
using Wisdom.Order.Infrastructure.Repositories;
namespace Wisdom.Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddNpgsqlDataSource(connectionString);
        services.AddDbContext<OrderDbContext>((sp, options) =>
            options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>(),
                b => b.MigrationsAssembly("Wisdom.Order.Infrastructure.Migration")));

        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

        return services;
    }
}
