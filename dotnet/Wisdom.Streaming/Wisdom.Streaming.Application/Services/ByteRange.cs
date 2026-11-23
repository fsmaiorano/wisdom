namespace Wisdom.Streaming.Application.Services;

/// <summary>Inclusive byte range.</summary>
public readonly record struct ByteRange(long Start, long End)
{
    public long Length => End - Start + 1;

    /// <summary>
    /// Parses a single "bytes=start-end" range. Supports open-ended (bytes=N-) and suffix (bytes=-N) forms.
    /// Multi-range requests and ranges outside the resource are not satisfiable.
    /// </summary>
    public static bool TryParse(string? header, long size, out ByteRange range)
    {
        range = default;

        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("bytes=", StringComparison.Ordinal))
            return false;

        var spec = header["bytes=".Length..];
        if (spec.Contains(','))
            return false;

        var parts = spec.Split('-', 2);
        if (parts.Length != 2)
            return false;

        long start, end;

        if (parts[0].Length == 0)
        {
            if (!long.TryParse(parts[1], out var suffix) || suffix <= 0)
                return false;

            start = Math.Max(0, size - suffix);
            end = size - 1;
        }
        else
        {
            if (!long.TryParse(parts[0], out start) || start < 0)
                return false;

            if (parts[1].Length == 0)
                end = size - 1;
            else if (!long.TryParse(parts[1], out end) || end < start)
                return false;
        }

        if (start >= size || end >= size || start > end)
            return false;

        range = new ByteRange(start, end);
        return true;
    }
}
