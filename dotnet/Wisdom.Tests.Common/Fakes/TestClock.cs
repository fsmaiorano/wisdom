namespace Wisdom.Tests.Common.Fakes;
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class TestClock : IClock
{
    private DateTimeOffset _current;

    public TestClock(DateTimeOffset? fixedTime = null)
        => _current = fixedTime ?? new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public DateTimeOffset UtcNow => _current;

    public void Advance(TimeSpan duration) => _current = _current.Add(duration);

    public void Set(DateTimeOffset time) => _current = time;
}
