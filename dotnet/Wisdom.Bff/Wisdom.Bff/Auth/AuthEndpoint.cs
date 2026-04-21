namespace Wisdom.Bff.Auth;

public static class AuthEndpoint
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/bff/auth/login", async (LoginRequest request, AuthHandler handler, CancellationToken ct) =>
        {
            var result = await handler.LoginAsync(request, ct);
            return result is null
                ? Results.Unauthorized()
                : Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("Authenticate via Keycloak and return access token + user info")
        .WithTags("BFF — Auth")
        .AllowAnonymous()
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
