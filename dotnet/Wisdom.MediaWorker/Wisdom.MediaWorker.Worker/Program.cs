using Wisdom.BuildingBlocks.Events;
using Wisdom.BuildingBlocks.Events.IntegrationEvents.Contracts;
using Wisdom.BuildingBlocks.Loggers;
using Wisdom.BuildingBlocks.Telemetry;
using Wisdom.MediaWorker.Application;
using Wisdom.MediaWorker.Application.Services;
using Wisdom.MediaWorker.Infrastructure;
using Wisdom.MediaWorker.Worker.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.AddCustomLoggers();
builder.AddWisdomTelemetry();

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddHostedService<EventBusSubscriptionService<MediaReadyIntegrationEvent, MediaWorkerHandler>>();
builder.Services.AddHostedService<MediaReconcilerWorker>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok()).WithTags("Health");

app.Run();

namespace MediaWorker.Worker
{
    public partial class MediaWorkerProgram;
}
