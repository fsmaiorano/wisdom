using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.Streaming.Application.Services;
using Wisdom.Streaming.Application.Settings;
namespace Wisdom.Streaming.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StreamingSettings>(configuration.GetSection(StreamingSettings.SectionName));
        services.AddScoped<StreamingService>();

        return services;
    }
}
