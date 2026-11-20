using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Wisdom.BuildingBlocks.Loggers;

public static class LoggerExtensions
{
    public static WebApplicationBuilder AddCustomLoggers(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Environment.ApplicationName;
        var seqUrl = builder.Configuration["Seq:ServerUrl"];

        var config = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            // The "Microsoft" override above also silences Microsoft.Hosting.Lifetime,
            // which is what logs "Now listening on: ..." / "Application started.".
            // Without this, a slow (not stuck) startup is indistinguishable from a
            // genuine hang — nothing prints between DB init and the first request.
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithProperty("LogType", "System")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] [CID:{CorrelationId}] [{LogType}] {Message:lj}{NewLine}{Exception}"
            );

        if (!string.IsNullOrWhiteSpace(seqUrl))
            config = config.WriteTo.Seq(seqUrl);

        Log.Logger = config.CreateLogger();

        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddLogging(b =>
        {
            b.ClearProviders();
            b.AddSerilog(dispose: true);
        });
        builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

        builder.Host.UseSerilog();

        return builder;
    }
}
