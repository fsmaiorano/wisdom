using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace Wisdom.Tests.Common.Auth;

public class FakeAuthHandlerOptions : AuthenticationSchemeOptions;

public class FakeAuthHandler(
    IOptionsMonitor<FakeAuthHandlerOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<FakeAuthHandlerOptions>(options, logger, encoder)
{
    public const string SchemeName = "FakeBearer";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var raw = authHeader.ToString();
        if (!raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.NoResult());

        var email = raw["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(email))
            return Task.FromResult(AuthenticateResult.Fail("Empty bearer value."));

        var userId = new Guid(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(email))).ToString();

        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim("email", email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        var identity  = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
