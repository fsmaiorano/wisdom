using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wisdom.Course.Application.Interfaces;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Infrastructure.Data;
using Wisdom.Course.Infrastructure.Repositories;
using Wisdom.Course.Infrastructure.Services;

namespace Wisdom.Course.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddNpgsqlDataSource(connectionString);
        services.AddDbContext<CourseDbContext>((sp, options) =>
            options.UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>(),
                b => b.MigrationsAssembly("Wisdom.Course.Infrastructure.Migration")));

        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        var mediaBaseUrl = configuration["Services:MediaApiBaseUrl"]
            ?? throw new InvalidOperationException("Services:MediaApiBaseUrl is not configured.");
        services.AddHttpClient<IMediaApiClient, MediaApiClient>(client =>
            client.BaseAddress = new Uri(mediaBaseUrl));

        return services;
    }
}
