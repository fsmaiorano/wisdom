using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.User.Application.UseCases;
namespace Wisdom.User.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddScoped<RegisterUserUseCase>();
        services.AddScoped<LoginUserUseCase>();
        services.AddScoped<RefreshTokenUseCase>();
        services.AddScoped<LogoutUserUseCase>();
        services.AddScoped<MeUseCase>();

        return services;
    }
}