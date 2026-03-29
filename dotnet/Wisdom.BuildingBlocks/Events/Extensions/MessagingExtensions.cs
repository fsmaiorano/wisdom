using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wisdom.BuildingBlocks.Events.Interfaces;

namespace Wisdom.BuildingBlocks.Events.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:RabbitMq is not configured. " +
                "Add it to appsettings.json: \"RabbitMq\": \"amqp://admin:admin@localhost:5672/\"");

        var options = configuration
            .GetSection(EventBusOptions.SectionName)
            .Get<EventBusOptions>() ?? new EventBusOptions();

        services.AddSingleton(options);

        services.AddSingleton<IEventBus>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<EventBus>>();
            return new EventBus(sp, logger, connectionString, options);
        });

        return services;
    }
}
