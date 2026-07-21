using Wisdom.BuildingBlocks.Auth;
using Wisdom.BuildingBlocks.Extensions;

namespace Wisdom.Bff.Endpoints;

/// <summary>
/// Byte-range video proxy to wisdom-streaming. Not a typed/JSON endpoint like the
/// others — the &lt;video&gt; element loads this URL directly (bypassing the SPA's auth
/// interceptor), so it stays anonymous just like it does today, while still
/// forwarding the internal API key (and user identity, when present) downstream.
/// </summary>
public static class StreamEndpoint
{
    public static IEndpointRouteBuilder MapStreamEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMethods("/stream/{mediaId}", ["GET", "HEAD"], async (
                string mediaId,
                HttpContext httpContext,
                IHttpClientFactory httpClientFactory,
                IConfiguration config,
                CancellationToken ct) =>
            {
                var streamApiBase = config["BackendServices:StreamApi"]!;
                var targetUrl = $"{streamApiBase}/stream/{mediaId}{httpContext.Request.QueryString}";

                using var request = new HttpRequestMessage(new HttpMethod(httpContext.Request.Method), targetUrl);

                if (httpContext.Request.Headers.TryGetValue("Range", out var range))
                    request.Headers.TryAddWithoutValidation("Range", (string?)range);

                var apiKey = config["InternalAuth:ApiKey"]!;
                request.Headers.TryAddWithoutValidation(InternalAuthHandler.ApiKeyHeader, apiKey);

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    var userId = httpContext.User.GetUserId()?.ToString();
                    var email = httpContext.User.GetEmail();
                    if (userId is not null)
                        request.Headers.TryAddWithoutValidation(InternalAuthHandler.UserIdHeader, userId);
                    if (email is not null)
                        request.Headers.TryAddWithoutValidation(InternalAuthHandler.UserEmailHeader, email);
                }

                var client = httpClientFactory.CreateClient("stream");
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                httpContext.Response.StatusCode = (int)response.StatusCode;

                foreach (var header in response.Headers)
                {
                    if (header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                        continue;
                    httpContext.Response.Headers[header.Key] = header.Value.ToArray();
                }

                foreach (var header in response.Content.Headers)
                    httpContext.Response.Headers[header.Key] = header.Value.ToArray();

                if (httpContext.Request.Method != HttpMethods.Head)
                    await response.Content.CopyToAsync(httpContext.Response.Body, ct);
            })
            .WithName("StreamMedia")
            .WithTags("Stream")
            .AllowAnonymous();

        return app;
    }
}
