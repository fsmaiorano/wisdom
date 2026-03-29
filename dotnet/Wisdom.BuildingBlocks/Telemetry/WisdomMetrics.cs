using System.Diagnostics.Metrics;

namespace Wisdom.BuildingBlocks.Telemetry;

public static class WisdomMetrics
{
    public const string MeterName = "Wisdom";

    private static readonly Meter _meter = new(MeterName, "1.0.0");

    /// <summary>Total use-case executions. Tag: use_case, module.</summary>
    public static readonly Counter<long> UseCaseExecutions =
        _meter.CreateCounter<long>("wisdom.usecase.executions", description: "Total use case executions");

    /// <summary>Failed use-case executions. Tag: use_case, module.</summary>
    public static readonly Counter<long> UseCaseFailures =
        _meter.CreateCounter<long>("wisdom.usecase.failures", description: "Total use case failures");

    /// <summary>Use-case execution duration in milliseconds. Tag: use_case, module.</summary>
    public static readonly Histogram<double> UseCaseDuration =
        _meter.CreateHistogram<double>("wisdom.usecase.duration", "ms", "Use case execution duration");
}
