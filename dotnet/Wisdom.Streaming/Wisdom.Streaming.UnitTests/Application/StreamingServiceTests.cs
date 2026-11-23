using Wisdom.Streaming.Application.Services;
using Wisdom.Streaming.UnitTests.Mocks;
namespace Wisdom.Streaming.UnitTests.Application;

public class StreamingServiceTests
{
    private readonly FakeMediaStatusReader _statusReader = new();
    private readonly FakeVideoStorage _storage = new();
    private readonly StreamingService _sut;

    public StreamingServiceTests()
    {
        _sut = new StreamingService(_statusReader, _storage);
    }

    [Theory]
    [InlineData("4k")]
    [InlineData("720")]
    [InlineData("../720p")]
    public async Task Resolve_WithInvalidQuality_ReturnsInvalidQuality(string quality)
    {
        var result = await _sut.ResolveAsync(Guid.NewGuid(), quality);

        Assert.Equal(StreamLookupStatus.InvalidQuality, result.Status);
    }

    [Fact]
    public async Task Resolve_WithUnknownMedia_ReturnsMediaNotFound()
    {
        var result = await _sut.ResolveAsync(Guid.NewGuid(), "720p");

        Assert.Equal(StreamLookupStatus.MediaNotFound, result.Status);
    }

    [Theory]
    [InlineData("Uploaded")]
    [InlineData("Processing")]
    [InlineData("Failed")]
    public async Task Resolve_WithMediaNotReady_ReturnsMediaNotReadyWithStatus(string status)
    {
        var id = Guid.NewGuid();
        _statusReader.Statuses[id] = status;

        var result = await _sut.ResolveAsync(id, "720p");

        Assert.Equal(StreamLookupStatus.MediaNotReady, result.Status);
        Assert.Equal(status, result.MediaStatus);
    }

    [Fact]
    public async Task Resolve_WhenFileIsMissing_ReturnsFileNotFound()
    {
        var id = Guid.NewGuid();
        _statusReader.Statuses[id] = "Ready";

        var result = await _sut.ResolveAsync(id, "720p");

        Assert.Equal(StreamLookupStatus.FileNotFound, result.Status);
    }

    [Theory]
    [InlineData(null, "720p")]
    [InlineData("", "720p")]
    [InlineData("480p", "480p")]
    [InlineData("1080p", "1080p")]
    public async Task Resolve_WithReadyMedia_ReturnsObjectPathAndSize(string? quality, string expectedQuality)
    {
        var id = Guid.NewGuid();
        _statusReader.Statuses[id] = "Ready";
        _storage.Objects[$"media/{id}/{expectedQuality}.mp4"] = new byte[123];

        var result = await _sut.ResolveAsync(id, quality);

        Assert.True(result.IsFound);
        Assert.Equal($"media/{id}/{expectedQuality}.mp4", result.ObjectPath);
        Assert.Equal(123, result.Size);
    }
}
