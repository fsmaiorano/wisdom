using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Wisdom.BuildingBlocks.Events.DomainEvents;
using Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;
using Wisdom.BuildingBlocks.Events.Idempotency;
using Wisdom.BuildingBlocks.Events.Outbox;
using Wisdom.Media.Application.Interfaces;
using Wisdom.Media.Domain.Repositories;
using Wisdom.Media.Infrastructure.Data;
using Wisdom.Media.Infrastructure.Repositories;
using Wisdom.Media.Infrastructure.Services;
using Wisdom.Media.Infrastructure.Settings;

namespace Wisdom.Media.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOutbox<MediaDbContext>();
        services.AddIdempotency<MediaDbContext>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<MediaDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Wisdom.Media.Infrastructure.Migration"));
            options.AddInterceptors(sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>());
        });

        services.AddScoped<IMediaRepository, MediaRepository>();

        var minioSettings = configuration.GetSection("Minio").Get<MinioSettings>() ?? new MinioSettings();
        services.AddSingleton<MinioSettings>(minioSettings);
        services.AddMinio(configureClient => configureClient
            .WithEndpoint(minioSettings.Endpoint)
            .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey)
            .WithSSL(minioSettings.UseSSL)
            .Build());

        services.AddScoped<IMinioService, MinioService>();

        return services;
    }
}
