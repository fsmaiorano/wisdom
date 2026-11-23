namespace Wisdom.Streaming.Application.Abstractions;

public interface IMediaStatusReader
{
    /// <summary>Returns the media status (e.g. "Ready"), or null when the media does not exist.</summary>
    Task<string?> GetStatusAsync(Guid mediaId, CancellationToken ct = default);
}
