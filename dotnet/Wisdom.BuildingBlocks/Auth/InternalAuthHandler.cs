using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Wisdom.BuildingBlocks.Auth;

/// <summary>
/// Authentication handler for internal service-to-service calls from the BFF.
///
/// Validates the X-Internal-Api-Key header to ensure requests come from the BFF.
/// Builds the user identity from X-User-Id and X-User-Email headers that the BFF
/// extracts from the validated JWT and forwards to backend services.
///
/// This allows backend services to remain completely isolated from Keycloak —
/// the BFF is the single JWT validation boundary.
/// </summary>
public sealed class InternalAuthHandler(
    IOptionsMonitor<InternalAuthOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<InternalAuthOptions>(options, logger, encoder)
{
    public const string SchemeName = "InternalAuth";

    // Headers set by the BFF before forwarding to backend services
    public const string ApiKeyHeader    = "X-Internal-Api-Key";
    public const string UserIdHeader    = "X-User-Id";
    public const string UserEmailHeader = "X-User-Email";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKey = Request.Headers[ApiKeyHeader].FirstOrDefault();

        // No API key — treat as anonymous (allows public endpoints to be called directly in dev)
        if (string.IsNullOrWhiteSpace(apiKey))
            return Task.FromResult(AuthenticateResult.NoResult());

        // Wrong API key — reject; request does not come from BFF
        if (apiKey != Options.ApiKey)
            return Task.FromResult(AuthenticateResult.Fail("Invalid internal API key."));

        // Build user identity from forwarded headers
        var claims = new List<Claim>();

        var userId = Request.Headers[UserIdHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim("sub", userId));
        }

        var email = Request.Headers[UserEmailHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim("email", email));
        }

        var identity  = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
