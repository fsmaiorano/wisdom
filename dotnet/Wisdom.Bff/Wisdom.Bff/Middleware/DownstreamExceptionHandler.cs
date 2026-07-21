using Microsoft.AspNetCore.Diagnostics;
using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Middleware;

/// <summary>
/// Translates a <see cref="DownstreamHttpException"/> raised by any typed client
/// into the exact status/body/content-type the downstream service returned, so
/// endpoints don't need a try/catch per route to preserve each service's error shape.
/// </summary>
public sealed class DownstreamExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not DownstreamHttpException ex)
            return false;

        httpContext.Response.StatusCode = ex.StatusCode;
        if (!string.IsNullOrEmpty(ex.Body))
        {
            httpContext.Response.ContentType = ex.ContentType ?? "application/json";
            await httpContext.Response.WriteAsync(ex.Body, ct);
        }

        return true;
    }
}
