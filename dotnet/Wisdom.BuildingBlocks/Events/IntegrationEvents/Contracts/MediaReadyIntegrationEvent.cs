namespace Wisdom.BuildingBlocks.Events.IntegrationEvents.Contracts;

/// <summary>
/// Published (via RabbitMQ) after a media file has been uploaded and processed.
/// Other modules (e.g. Course, Learning) subscribe to this event to react to
/// new media being available.
/// </summary>
public sealed class MediaReadyIntegrationEvent : IntegrationEvent
{
    public string MediaId { get; init; } = string.Empty;
    public string LessonId { get; init; } = string.Empty;
    public string MediaType { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
}