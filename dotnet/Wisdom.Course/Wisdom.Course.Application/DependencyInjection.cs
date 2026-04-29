using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.Course.Application.UseCases;
namespace Wisdom.Course.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<CreateCourseUseCase>();
        services.AddScoped<GetCourseUseCase>();
        services.AddScoped<ListCoursesUseCase>();
        services.AddScoped<GetTeacherCoursesUseCase>();
        services.AddScoped<UpdateCourseUseCase>();
        services.AddScoped<PublishCourseUseCase>();

        services.AddScoped<CreateLessonUseCase>();
        services.AddScoped<GetLessonUseCase>();
        services.AddScoped<GetLessonsUseCase>();
        services.AddScoped<UpdateLessonUseCase>();
        services.AddScoped<DeleteLessonUseCase>();

        services.AddScoped<CreateTagUseCase>();
        services.AddScoped<GetTagsUseCase>();
        services.AddScoped<AddTagToCourseUseCase>();

        return services;
    }
}
