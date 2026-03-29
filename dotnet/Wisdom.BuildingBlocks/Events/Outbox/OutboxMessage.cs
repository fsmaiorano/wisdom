namespace Wisdom.BuildingBlocks.Events.Outbox;

public class OutboxMessage(
    Guid id,
    DateTime occurredOn,
    string type,
    string content)
{
    public Guid Id { get; init; } = id;
    public DateTime OccurredOn { get; init; } = occurredOn;
    public string Type { get; init; } = type;
    public string Content { get; init; } = content;
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }

    /// <summary>Number of delivery attempts made so far.</summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// When non-null, the processor skips this message until after this timestamp.
    /// Used to implement exponential back-off between retries.
    /// </summary>
    public DateTime? ScheduledAfter { get; set; }
}
