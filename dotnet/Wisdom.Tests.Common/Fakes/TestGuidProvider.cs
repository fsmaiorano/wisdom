namespace Wisdom.Tests.Common.Fakes;
public interface IGuidProvider
{
    Guid NewGuid();
}

public sealed class TestGuidProvider : IGuidProvider
{
    private readonly Queue<Guid> _queue = new();
    private byte _sequence;

    public void Enqueue(params Guid[] guids)
    {
        foreach (var g in guids)
            _queue.Enqueue(g);
    }

    public Guid NewGuid()
        => _queue.TryDequeue(out var guid)
            ? guid
            : new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ++_sequence);
}
