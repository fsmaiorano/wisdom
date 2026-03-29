using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wisdom.BuildingBlocks.Events.Interfaces;
namespace Wisdom.BuildingBlocks.Events.Extensions;

public static class MessagingExtensions
{

    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMq")
                                       ?? throw new InvalidOperationException("RabbitMQ:ConnectionString is not configured");

        services.AddSingleton<IEventBus>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<EventBus>>();
            return new EventBus(sp, logger, rabbitMqConnectionString);
        });

        return services;
    }
}
