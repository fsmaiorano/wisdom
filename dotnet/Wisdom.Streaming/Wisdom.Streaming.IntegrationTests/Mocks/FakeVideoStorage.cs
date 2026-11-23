using Wisdom.Streaming.Application.Abstractions;
namespace Wisdom.Streaming.IntegrationTests.Mocks;

public class FakeVideoStorage : IVideoStorage
{
    public Dictionary<string, byte[]> Objects { get; } = [];

    public Task<long?> GetSizeAsync(string objectPath, CancellationToken ct = default)
        => Task.FromResult<long?>(Objects.TryGetValue(objectPath, out var data) ? data.Length : null);

    public async Task CopyRangeAsync(string objectPath, long offset, long length, Stream destination,
        CancellationToken ct = default)
        => await destination.WriteAsync(Objects[objectPath].AsMemory((int)offset, (int)length), ct);
}
