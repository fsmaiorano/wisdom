namespace Wisdom.Streaming.Application.Settings;

public class StreamingSettings
{
    public const string SectionName = "Streaming";

    /// <summary>Skips the internal API key check. Intended for local development only.</summary>
    public bool DisableAuth { get; set; }
}
