namespace Wisdom.BuildingBlocks.Events;

public class EventBusOptions
{
    public const string SectionName = "EventBus";

    /// <summary>Prefix applied to all exchange and queue names. Default: "wisdom".</summary>
    public string Prefix { get; set; } = "wisdom";

    /// <summary>Number of unacknowledged messages each consumer may hold. Default: 10.</summary>
    public ushort PrefetchCount { get; set; } = 10;

    /// <summary>TTL (ms) applied to messages in dead-letter queues. Default: 7 days.</summary>
    public int DeadLetterTtlMs { get; set; } = 604_800_000;
}
