using Wisdom.BuildingBlocks.Auth;
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

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomLoggers();
builder.AddWisdomTelemetry();

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddCorrelationIdServices();

builder.Services.AddInternalAuth(builder.Configuration);

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.InitialiseDatabaseAsync<CourseDbContext>();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

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

app.MapCourseEndpoints();
app.MapLessonEndpoints();
app.MapTagEndpoints();

app.Run();

namespace Course.Api
{
    public partial class CourseProgram;
}
