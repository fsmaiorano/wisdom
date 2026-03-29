using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Wisdom.BuildingBlocks.Telemetry;

public static class TelemetryExtensions
{
    public static WebApplicationBuilder AddWisdomTelemetry(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = builder.Configuration[$"{TelemetrySettings.SectionName}:OtlpEndpoint"];
        var enableConsole = string.Equals(
            builder.Configuration[$"{TelemetrySettings.SectionName}:EnableConsoleExporter"],
            "true", StringComparison.OrdinalIgnoreCase);

        var settings = new TelemetrySettings { OtlpEndpoint = otlpEndpoint, EnableConsoleExporter = enableConsole };

        var serviceName = builder.Environment.ApplicationName;
        var environment = builder.Environment.EnvironmentName;

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes([
                    new KeyValuePair<string, object>("deployment.environment", environment)
                ]))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                    .AddHttpClientInstrumentation()
                    .AddSource(WisdomActivitySource.Name);

                if (settings.EnableConsoleExporter)
                    tracing.AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(settings.OtlpEndpoint))
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(settings.OtlpEndpoint));
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter(WisdomMetrics.MeterName);

                if (settings.EnableConsoleExporter)
                    metrics.AddConsoleExporter();

                if (!string.IsNullOrWhiteSpace(settings.OtlpEndpoint))
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(settings.OtlpEndpoint));
            });

        return builder;
    }
}
