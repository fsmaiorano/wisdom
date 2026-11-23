using Wisdom.BuildingBlocks.Events.IntegrationEvents.Contracts;
using Wisdom.MediaWorker.Application.Services;
using Wisdom.MediaWorker.UnitTests.Mocks;
using Wisdom.Tests.Common.Fakes;
namespace Wisdom.MediaWorker.UnitTests.Application;

public class MediaProcessorTests
{
    private readonly FakeMediaRepository _repository = new();
    private readonly FakeObjectStorage _storage = new();
    private readonly FakeVideoTranscoder _transcoder = new();
    private readonly MediaProcessor _sut;

    public MediaProcessorTests()
    {
        _sut = new MediaProcessor(_repository, _storage, _transcoder, new FakeAppLogger<MediaProcessor>());
    }

    private static MediaReadyIntegrationEvent NewEvent(string? mediaId = null) => new()
    {
        MediaId = mediaId ?? Guid.NewGuid().ToString(),
        LessonId = Guid.NewGuid().ToString(),
        MediaType = "Video",
        FilePath = "raw/lesson.mp4"
    };

    [Fact]
    public async Task Process_Success_MarksReadyAndUploadsAllRenditions()
    {
        var @event = NewEvent();

        await _sut.ProcessAsync(@event);

        var mediaId = Guid.Parse(@event.MediaId);
        Assert.Equal(["Processing", "Ready"], _repository.StatusChanges.Select(s => s.Status));
        Assert.Equal((mediaId, $"media/{mediaId}/thumbnail.jpg", 42), _repository.ReadyCall);
        Assert.Equal("image/jpeg", _storage.Uploaded[$"media/{mediaId}/thumbnail.jpg"]);
        Assert.Equal("video/mp4", _storage.Uploaded[$"media/{mediaId}/480p.mp4"]);
        Assert.Equal("video/mp4", _storage.Uploaded[$"media/{mediaId}/720p.mp4"]);
        Assert.Equal("video/mp4", _storage.Uploaded[$"media/{mediaId}/1080p.mp4"]);
        Assert.Equal(["raw/lesson.mp4"], _storage.Deleted);
    }

    [Fact]
    public async Task Process_DownloadFails_SetsFailedAndDoesNotThrow()
    {
        _storage.FailDownload = true;

        await _sut.ProcessAsync(NewEvent());

        Assert.Equal("Failed", _repository.LastStatus);
        Assert.Null(_repository.ReadyCall);
        Assert.Empty(_storage.Uploaded);
        Assert.Empty(_storage.Deleted);
    }

    [Fact]
    public async Task Process_TranscodeFails_SetsFailedAndKeepsRawFile()
    {
        _transcoder.Fail = true;

        await _sut.ProcessAsync(NewEvent());

        Assert.Equal("Failed", _repository.LastStatus);
        Assert.Empty(_storage.Deleted);
    }

    [Fact]
    public async Task Process_RawFileDeleteFails_StillEndsReady()
    {
        _storage.FailDelete = true;

        await _sut.ProcessAsync(NewEvent());

        Assert.Equal("Ready", _repository.LastStatus);
    }

    [Fact]
    public async Task Process_CannotSetProcessing_StopsWithoutDownloading()
    {
        _repository.ThrowOnSetProcessing = true;

        await _sut.ProcessAsync(NewEvent());

        Assert.Empty(_storage.Downloaded);
        Assert.Empty(_repository.StatusChanges);
    }

    [Fact]
    public async Task Process_InvalidMediaId_IsIgnored()
    {
        await _sut.ProcessAsync(NewEvent("not-a-guid"));

        Assert.Empty(_repository.StatusChanges);
        Assert.Empty(_storage.Downloaded);
    }
}
