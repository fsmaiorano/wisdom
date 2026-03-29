using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;
using Wisdom.BuildingBlocks.Services.Correlation;
namespace Wisdom.BuildingBlocks.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context, ICorrelationIdService correlationIdService)
    {
        string correlationId;

        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationIdHeader))
        {
            correlationId = Guid.CreateVersion7().ToString();
            context.Request.Headers[CorrelationIdHeader] = correlationId;
        }
        else
        {
            correlationId = correlationIdHeader.ToString();
        }

        correlationIdService.SetCorrelationId(correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("correlation.id", correlationId);
            activity.AddBaggage("CorrelationId", correlationId);
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}