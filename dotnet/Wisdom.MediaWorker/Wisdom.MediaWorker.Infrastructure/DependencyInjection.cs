using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Wisdom.BuildingBlocks.Events.Extensions;
using Wisdom.MediaWorker.Application.Abstractions;
using Wisdom.MediaWorker.Infrastructure.Persistence;
using Wisdom.MediaWorker.Infrastructure.Settings;
using Wisdom.MediaWorker.Infrastructure.Storage;
using Wisdom.MediaWorker.Infrastructure.Video;
namespace Wisdom.MediaWorker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEventBus(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddNpgsqlDataSource(connectionString);
        services.AddScoped<IMediaRepository, MediaRepository>();

        var storageSettings = configuration.GetSection("Storage").Get<StorageSettings>() ?? new StorageSettings();
        services.AddSingleton(storageSettings);
        services.AddMinio(configureClient => configureClient
            .WithEndpoint(storageSettings.Endpoint)
            .WithCredentials(storageSettings.AccessKey, storageSettings.SecretKey)
            .WithSSL(storageSettings.UseSSL)
            .Build());
        services.AddScoped<IObjectStorage, S3ObjectStorage>();

        services.AddSingleton<IVideoTranscoder, FfmpegVideoTranscoder>();

        return services;
    }
}
