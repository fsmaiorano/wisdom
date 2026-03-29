using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.Learning.Application.UseCases;
namespace Wisdom.Learning.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddScoped<UpdateProgressUseCase>();
        services.AddScoped<GetProgressUseCase>();
        services.AddScoped<GetCourseProgressUseCase>();

        return services;
    }
}
