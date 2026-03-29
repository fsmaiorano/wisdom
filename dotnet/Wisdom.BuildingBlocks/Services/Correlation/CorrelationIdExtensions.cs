using Microsoft.Extensions.DependencyInjection;
using Wisdom.BuildingBlocks.Http;
namespace Wisdom.BuildingBlocks.Services.Correlation;

public static class CorrelationIdExtensions
{

    public static IServiceCollection AddCorrelationIdServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdService, CorrelationIdService>();
        services.AddScoped<CorrelationIdHandler>();
        services.AddTransient<LoggingHandler>();
        return services;
    }
}
