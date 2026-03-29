using Wisdom.BuildingBlocks.Auth;
using Wisdom.BuildingBlocks.Exceptions;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.BuildingBlocks.Middlewares;
using Wisdom.BuildingBlocks.Services;
using Wisdom.BuildingBlocks.Services.Correlation;
using Wisdom.BuildingBlocks.Telemetry;
using Wisdom.Order.Api.Endpoints;
using Wisdom.Order.Application;
using Wisdom.Order.Infrastructure;
using Wisdom.Order.Infrastructure.Data;

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
    await app.InitialiseDatabaseAsync<OrderDbContext>();
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

app.MapOrderEndpoints();

app.Run();

namespace Order.Api
{
    public partial class OrderProgram;
}
