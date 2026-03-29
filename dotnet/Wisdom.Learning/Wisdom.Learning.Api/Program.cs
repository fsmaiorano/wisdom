using Wisdom.BuildingBlocks.Auth;
using Wisdom.BuildingBlocks.Exceptions;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.BuildingBlocks.Middlewares;
using Wisdom.BuildingBlocks.Services;
using Wisdom.BuildingBlocks.Services.Correlation;
using Wisdom.BuildingBlocks.Telemetry;
using Wisdom.Learning.Api.Endpoints;
using Wisdom.Learning.Application;
using Wisdom.Learning.Infrastructure;
using Wisdom.Learning.Infrastructure.Data;

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.InitialiseDatabaseAsync<LearningDbContext>();
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

app.MapProgressEndpoints();

app.Run();

namespace Learning.Api
{
    public partial class LearningProgram;
}
