using Wisdom.MediaWorker.Application.Abstractions;
using Wisdom.MediaWorker.Application.Services;
using Wisdom.MediaWorker.UnitTests.Mocks;
using Wisdom.Tests.Common.Fakes;
namespace Wisdom.MediaWorker.UnitTests.Application;

public class MediaReconcilerTests
{
    private readonly FakeMediaRepository _repository = new();
    private readonly FakeObjectStorage _storage = new();
    private readonly MediaReconciler _sut;

    public MediaReconcilerTests()
    {
        var processor = new MediaProcessor(_repository, _storage, new FakeVideoTranscoder(),
            new FakeAppLogger<MediaProcessor>());
        _sut = new MediaReconciler(_repository, processor, new FakeAppLogger<MediaReconciler>());
    }

    [Fact]
    public async Task RunOnce_WithNoStaleMedia_DoesNothing()
    {
        await _sut.RunOnceAsync(TimeSpan.FromMinutes(5));

        Assert.Empty(_repository.StatusChanges);
        Assert.Empty(_storage.Downloaded);
    }

    [Fact]
    public async Task RunOnce_UsesStaleAfterToComputeCutoff()
    {
        var before = DateTime.UtcNow;

        await _sut.RunOnceAsync(TimeSpan.FromMinutes(5));

        Assert.NotNull(_repository.ClaimCutoff);
        Assert.InRange(_repository.ClaimCutoff!.Value, before.AddMinutes(-5.1), DateTime.UtcNow.AddMinutes(-4.9));
    }

    [Fact]
    public async Task RunOnce_ReprocessesEveryClaimedMedia()
    {
        var first = new StaleMedia(Guid.NewGuid(), Guid.NewGuid(), "Video", "raw/a.mp4");
        var second = new StaleMedia(Guid.NewGuid(), Guid.NewGuid(), "Video", "raw/b.mp4");
        _repository.StaleToReturn.AddRange([first, second]);

        await _sut.RunOnceAsync(TimeSpan.FromMinutes(5));

        Assert.Equal(["raw/a.mp4", "raw/b.mp4"], _storage.Downloaded);
        Assert.Contains(_repository.StatusChanges, s => s.MediaId == first.Id && s.Status == "Ready");
        Assert.Contains(_repository.StatusChanges, s => s.MediaId == second.Id && s.Status == "Ready");
    }
}
