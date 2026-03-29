using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace Wisdom.BuildingBlocks.Events.Outbox;

public static class OutboxExtensions
{
    public static IServiceCollection AddOutbox<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext, IOutboxDbContext
    {
        services.AddScoped<ConvertDomainEventsToOutboxMessagesInterceptor>();
        services.AddHostedService<OutboxProcessor<TDbContext>>();
        return services;
    }
}

