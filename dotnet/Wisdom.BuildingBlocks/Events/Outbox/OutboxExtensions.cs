using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wisdom.BuildingBlocks.Events.Outbox;

public static class OutboxExtensions
{
    public static IServiceCollection AddOutbox<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TDbContext : DbContext, IOutboxDbContext
    {
        services.Configure<OutboxSettings>(configuration.GetSection(OutboxSettings.SectionName));
        services.AddScoped<ConvertDomainEventsToOutboxMessagesInterceptor>();
        services.AddHostedService<OutboxProcessor<TDbContext>>();
        return services;
    }
}
