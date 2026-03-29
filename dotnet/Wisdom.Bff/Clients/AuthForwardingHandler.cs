using Wisdom.BuildingBlocks.Auth;
using Wisdom.BuildingBlocks.Extensions;

namespace Wisdom.Bff.Clients;

/// <summary>
/// Translates the incoming JWT identity into internal service-to-service headers
/// before forwarding requests from BFF aggregation endpoints to backend services.
///
/// The BFF owns JWT validation. Backend services never see the raw JWT —
/// they receive the user identity via X-User-Id / X-User-Email headers,
/// authenticated by the shared X-Internal-Api-Key.
/// </summary>
public sealed class AuthForwardingHandler(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : DelegatingHandler
{
    private readonly string _apiKey =
        configuration["InternalAuth:ApiKey"]
        ?? throw new InvalidOperationException("InternalAuth:ApiKey is required.");

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Strip the raw JWT — backend services must not re-validate it
        request.Headers.Remove("Authorization");

        // Add the internal API key so services know this comes from BFF
        request.Headers.TryAddWithoutValidation(InternalAuthHandler.ApiKeyHeader, _apiKey);

        // Forward user identity extracted from the already-validated JWT
        var context = httpContextAccessor.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true) return base.SendAsync(request, cancellationToken);
        var userId = context.User.GetUserId()?.ToString();
        var email = context.User.GetEmail();

        if (userId is not null)
            request.Headers.TryAddWithoutValidation(InternalAuthHandler.UserIdHeader, userId);
        if (email is not null)
            request.Headers.TryAddWithoutValidation(InternalAuthHandler.UserEmailHeader, email);

        return base.SendAsync(request, cancellationToken);
    }
}