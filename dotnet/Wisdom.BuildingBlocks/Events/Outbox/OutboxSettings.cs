namespace Wisdom.BuildingBlocks.Events.Outbox;

public class OutboxSettings
{
    public const string SectionName = "Outbox";

    /// <summary>How often the processor polls for pending messages (seconds). Default: 10.</summary>
    public int IntervalSeconds { get; set; } = 10;

    /// <summary>Maximum number of messages processed per polling cycle. Default: 20.</summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>
    /// Maximum delivery attempts before a message is permanently dead-lettered.
    /// Default: 5.
    /// </summary>
    public int MaxRetries { get; set; } = 5;
}
