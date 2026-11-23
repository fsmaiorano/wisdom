using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Wisdom.Streaming.Application.Abstractions;
using Wisdom.Streaming.Infrastructure.Persistence;
using Wisdom.Streaming.Infrastructure.Storage;
namespace Wisdom.Streaming.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddNpgsqlDataSource(connectionString);
        services.AddScoped<IMediaStatusReader, MediaStatusReader>();

        var storageSettings = configuration.GetSection("Storage").Get<StorageSettings>() ?? new StorageSettings();
        services.AddSingleton(storageSettings);
        services.AddMinio(configureClient => configureClient
            .WithEndpoint(storageSettings.Endpoint)
            .WithCredentials(storageSettings.AccessKey, storageSettings.SecretKey)
            .WithSSL(storageSettings.UseSSL)
            .Build());
        services.AddScoped<IVideoStorage, S3VideoStorage>();

        return services;
    }
}
