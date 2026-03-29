using Microsoft.AspNetCore.Builder;
using Serilog;
namespace Wisdom.BuildingBlocks.Middlewares;

public static class RequestLoggingExtensions
{

    public static IApplicationBuilder UseStructuredRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
            {
                diagCtx.Set("CorrelationId", httpCtx.Request.Headers["X-Correlation-ID"].ToString());
                diagCtx.Set("RemoteIpAddress", httpCtx.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            };
        });
    }
}
