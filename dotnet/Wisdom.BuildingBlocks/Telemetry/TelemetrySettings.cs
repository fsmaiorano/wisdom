namespace Wisdom.BuildingBlocks.Telemetry;

public class TelemetrySettings
{
    public const string SectionName = "Telemetry";

    public string? OtlpEndpoint { get; init; }
    public bool EnableConsoleExporter { get; init; }
}
