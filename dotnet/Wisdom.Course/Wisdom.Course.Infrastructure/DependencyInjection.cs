using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Infrastructure.Data;
using Wisdom.Course.Infrastructure.Repositories;
namespace Wisdom.Course.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<CourseDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Wisdom.Course.Infrastructure.Migration")));

        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        return services;
    }
}
