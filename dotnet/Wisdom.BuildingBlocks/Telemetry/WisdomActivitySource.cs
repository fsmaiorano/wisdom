using System.Diagnostics;

namespace Wisdom.BuildingBlocks.Telemetry;

public static class WisdomActivitySource
{
    public const string Name = "Wisdom";

    public static readonly ActivitySource Instance = new(Name, "1.0.0");
}
