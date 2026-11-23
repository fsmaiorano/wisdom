using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Wisdom.BuildingBlocks.Events.IntegrationEvents.Contracts;
using Wisdom.BuildingBlocks.Events.IntegrationEvents.Interfaces;
using Wisdom.MediaWorker.Application.Services;
using Wisdom.MediaWorker.IntegrationTests.Fixtures;
namespace Wisdom.MediaWorker.IntegrationTests.Worker;

public class MediaWorkerTests(MediaWorkerFixture fixture) : IClassFixture<MediaWorkerFixture>
{
    private static MediaReadyIntegrationEvent NewEvent() => new()
    {
        MediaId = Guid.NewGuid().ToString(),
        LessonId = Guid.NewGuid().ToString(),
        MediaType = "Video",
        FilePath = $"raw/{Guid.NewGuid():N}.mp4"
    };

    [Fact]
    public async Task Health_Returns200()
    {
        var response = await fixture.DoGet("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void Handler_IsRegisteredForMediaReadyEvent()
    {
        using var scope = fixture.Services.CreateScope();

        var handler = scope.ServiceProvider.GetService<MediaWorkerHandler>();

        Assert.IsAssignableFrom<IIntegrationEventHandler<MediaReadyIntegrationEvent>>(handler);
    }

    [Fact]
    public async Task Handler_WithMediaReadyEvent_ProcessesMediaUntilReady()
    {
        var @event = NewEvent();
        using var scope = fixture.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<MediaWorkerHandler>();

        await handler.HandleAsync(@event);

        var mediaId = Guid.Parse(@event.MediaId);
        Assert.Contains((mediaId, "Processing"), fixture.Repository.StatusChanges);
        Assert.Contains((mediaId, "Ready"), fixture.Repository.StatusChanges);
        Assert.Contains($"media/{mediaId}/720p.mp4", fixture.Storage.Uploaded.Keys);
        Assert.Contains(@event.FilePath, fixture.Storage.Deleted);
    }

    [Fact]
    public async Task Handler_WhenTranscodeFails_MarksMediaFailed()
    {
        var @event = NewEvent();
        fixture.Transcoder.Fail = true;
        try
        {
            using var scope = fixture.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<MediaWorkerHandler>();

            await handler.HandleAsync(@event);

            Assert.Contains((Guid.Parse(@event.MediaId), "Failed"), fixture.Repository.StatusChanges);
        }
        finally
        {
            fixture.Transcoder.Fail = false;
        }
    }
}
