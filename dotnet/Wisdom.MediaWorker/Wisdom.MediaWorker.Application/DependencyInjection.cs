using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.MediaWorker.Application.Services;
using Wisdom.MediaWorker.Application.Settings;
namespace Wisdom.MediaWorker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ReconcilerSettings>(configuration.GetSection(ReconcilerSettings.SectionName));

        services.AddScoped<MediaProcessor>();
        services.AddScoped<MediaReconciler>();
        services.AddScoped<MediaWorkerHandler>();

        return services;
    }
}
