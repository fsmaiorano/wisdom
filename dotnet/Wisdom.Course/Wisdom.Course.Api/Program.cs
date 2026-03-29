using Microsoft.EntityFrameworkCore;
using Minio;
using Wisdom.BuildingBlocks.Auth;
using Wisdom.BuildingBlocks.Events.Outbox;
using Wisdom.BuildingBlocks.Exceptions;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.BuildingBlocks.Middlewares;
using Wisdom.BuildingBlocks.Services;
using Wisdom.BuildingBlocks.Services.Correlation;
using Wisdom.BuildingBlocks.Telemetry;
using Wisdom.Course.Api.Endpoints;
using Wisdom.Course.Application;
using Wisdom.Course.Infrastructure;
using Wisdom.Course.Infrastructure.Data;
using Wisdom.Media.Application.Interfaces;
using Wisdom.Media.Application.UseCases;
using Wisdom.Media.Domain.Repositories;
using Wisdom.Media.Infrastructure.Data;
using Wisdom.Media.Infrastructure.Repositories;
using Wisdom.Media.Infrastructure.Services;
using Wisdom.Media.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomLoggers();
builder.AddWisdomTelemetry();

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddCorrelationIdServices();

// Media services — needed to upload lesson videos
var mediaConnectionString = builder.Configuration.GetConnectionString("MediaConnection");
builder.Services.AddScoped<ConvertDomainEventsToOutboxMessagesInterceptor>();
builder.Services.AddDbContext<MediaDbContext>((sp, options) =>
{
    options.UseNpgsql(mediaConnectionString);
    options.AddInterceptors(sp.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>());
});
builder.Services.AddScoped<IMediaRepository, MediaRepository>();

var minioSettings = builder.Configuration.GetSection("Minio").Get<MinioSettings>() ?? new MinioSettings();
builder.Services.AddSingleton(minioSettings);
builder.Services.AddMinio(c => c
    .WithEndpoint(minioSettings.Endpoint)
    .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey)
    .WithSSL(minioSettings.UseSSL)
    .Build());
builder.Services.AddScoped<IMinioService, MinioService>();
builder.Services.AddScoped<UploadMediaUseCase>();

builder.Services.AddInternalAuth(builder.Configuration);

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.InitialiseDatabaseAsync<CourseDbContext>();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseStructuredRequestLogging();
app.UseExceptionHandler(options => { });

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/debug/claims", (System.Security.Claims.ClaimsPrincipal user) =>
        Results.Ok(user.Claims.Select(c => new { c.Type, c.Value })))
        .RequireAuthorization()
        .WithTags("Debug");
}

app.MapCourseEndpoints();
app.MapLessonEndpoints();
app.MapTagEndpoints();

app.Run();

namespace Course.Api
{
    public partial class CourseProgram;
}
