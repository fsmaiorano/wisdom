namespace Wisdom.Streaming.Application.Abstractions;

public interface IVideoStorage
{
    /// <summary>Returns the object size in bytes, or null when the object does not exist.</summary>
    Task<long?> GetSizeAsync(string objectPath, CancellationToken ct = default);

    /// <summary>Streams <paramref name="length"/> bytes starting at <paramref name="offset"/> into <paramref name="destination"/>.</summary>
    Task CopyRangeAsync(string objectPath, long offset, long length, Stream destination, CancellationToken ct = default);
}
