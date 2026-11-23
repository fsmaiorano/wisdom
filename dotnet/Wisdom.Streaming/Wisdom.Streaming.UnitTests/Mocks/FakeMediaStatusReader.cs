using Wisdom.Streaming.Application.Abstractions;
namespace Wisdom.Streaming.UnitTests.Mocks;

public class FakeMediaStatusReader : IMediaStatusReader
{
    public Dictionary<Guid, string> Statuses { get; } = [];

    public Task<string?> GetStatusAsync(Guid mediaId, CancellationToken ct = default)
        => Task.FromResult(Statuses.GetValueOrDefault(mediaId));
}
