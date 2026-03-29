using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.BuildingBlocks.Events.DomainEvents.Interfaces;
using Wisdom.Media.Application.EventHandlers;
using Wisdom.Media.Application.UseCases;
using Wisdom.Media.Domain.Events;
namespace Wisdom.Media.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<GetMediaUseCase>();
        services.AddScoped<GetLessonMediaUseCase>();
        services.AddScoped<UpdateMediaStatusUseCase>();
        services.AddScoped<UploadMediaUseCase>();

        services.AddScoped<IDomainEventHandler<MediaReadyDomainEvent>, MediaReadyDomainEventHandler>();

        return services;
    }
}
