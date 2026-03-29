using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wisdom.Learning.Domain.Repositories;
using Wisdom.Learning.Infrastructure.Data;
using Wisdom.Learning.Infrastructure.Repositories;
namespace Wisdom.Learning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddNpgsqlDataSource(connectionString);
        services.AddDbContext<LearningDbContext>((sp, options) =>
            options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>(),
                b => b.MigrationsAssembly("Wisdom.Learning.Infrastructure.Migration")));

        services.AddScoped<IProgressRepository, ProgressRepository>();

        return services;
    }
}
