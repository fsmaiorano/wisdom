using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.Order.Application.UseCases;
namespace Wisdom.Order.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<PurchaseCourseUseCase>();
        services.AddScoped<GetEnrollmentsUseCase>();
        services.AddScoped<CheckEnrollmentUseCase>();

        return services;
    }
}
