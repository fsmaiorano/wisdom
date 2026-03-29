using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wisdom.User.Application.Interfaces;
using Wisdom.User.Domain.Repositories;
using Wisdom.User.Infrastructure.Data;
using Wisdom.User.Infrastructure.Repositories;
using Wisdom.User.Infrastructure.Services;
using Wisdom.User.Infrastructure.Settings;
namespace Wisdom.User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddNpgsqlDataSource(connectionString);
        services.AddDbContext<UserDbContext>((sp, options) =>
            options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>(),
                b => b.MigrationsAssembly("Wisdom.User.Infrastructure.Migration")));

        services.AddScoped<IUserRepository, UserRepository>();

        var keycloakSettings = new KeycloakSettings
        {
            BaseUrl = configuration["Keycloak:BaseUrl"] ?? string.Empty,
            Realm = configuration["Keycloak:Realm"] ?? string.Empty,
            ClientId = configuration["Keycloak:ClientId"] ?? string.Empty,
            ClientSecret = configuration["Keycloak:ClientSecret"] ?? string.Empty,
            AdminUsername = configuration["Keycloak:AdminUsername"] ?? string.Empty,
            AdminPassword = configuration["Keycloak:AdminPassword"] ?? string.Empty
        };
        services.AddSingleton(keycloakSettings);

        services.AddHttpClient<IKeycloakService, KeycloakService>(client =>
        {
            client.BaseAddress = new Uri(keycloakSettings.BaseUrl);
        });

        return services;
    }
}