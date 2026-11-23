using Wisdom.Streaming.Application.Services;
namespace Wisdom.Streaming.UnitTests.Application;

public class ByteRangeTests
{
    [Theory]
    [InlineData("bytes=0-99", 1000, 0, 99)]
    [InlineData("bytes=100-199", 1000, 100, 199)]
    [InlineData("bytes=500-", 1000, 500, 999)]
    [InlineData("bytes=-100", 1000, 900, 999)]
    [InlineData("bytes=-5000", 1000, 0, 999)]
    [InlineData("bytes=0-999", 1000, 0, 999)]
    public void TryParse_ValidRange_ReturnsInclusiveBounds(string header, long size, long start, long end)
    {
        var ok = ByteRange.TryParse(header, size, out var range);

        Assert.True(ok);
        Assert.Equal(new ByteRange(start, end), range);
        Assert.Equal(end - start + 1, range.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("items=0-10")]
    [InlineData("bytes=0-10,20-30")]
    [InlineData("bytes=abc-10")]
    [InlineData("bytes=10-abc")]
    [InlineData("bytes=-0")]
    [InlineData("bytes=-abc")]
    [InlineData("bytes=100-50")]
    [InlineData("bytes=1000-")]
    [InlineData("bytes=0-1000")]
    [InlineData("bytes=5")]
    public void TryParse_InvalidOrUnsatisfiable_ReturnsFalse(string? header)
    {
        Assert.False(ByteRange.TryParse(header, 1000, out _));
    }
}
