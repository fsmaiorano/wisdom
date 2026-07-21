using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Endpoints;

/// <summary>
/// Proxies Wisdom.User.Api's own /api/auth/* endpoints (register/login/refresh/logout).
/// Distinct from the BFF-native /bff/auth/login, which exchanges credentials with
/// Keycloak directly for the Angular app's session.
/// </summary>
public static class UserAuthEndpoints
{
    public static IEndpointRouteBuilder MapUserAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("User Auth (proxy)").AllowAnonymous();

        group.MapPost("/register", async (RegisterUserRequest request, IUserApiClient client, CancellationToken ct) =>
        {
            var result = await client.RegisterAsync(request, ct);
            return Results.Created($"/api/users/{result.UserId}", result);
        })
        .WithName("RegisterUser")
        .Produces<BackendRegisteredUser>(StatusCodes.Status201Created);

        group.MapPost("/login", async (LoginUserRequest request, IUserApiClient client, CancellationToken ct) =>
            Results.Ok(await client.LoginAsync(request, ct)))
        .WithName("ProxyLoginUser")
        .Produces<BackendAuthTokens>(StatusCodes.Status200OK);

        group.MapPost("/refresh", async (RefreshTokenRequest request, IUserApiClient client, CancellationToken ct) =>
            Results.Ok(await client.RefreshAsync(request, ct)))
        .WithName("RefreshToken")
        .Produces<BackendAuthTokens>(StatusCodes.Status200OK);

        group.MapPost("/logout", async (LogoutUserRequest request, IUserApiClient client, CancellationToken ct) =>
        {
            await client.LogoutAsync(request, ct);
            return Results.NoContent();
        })
        .WithName("LogoutUser")
        .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}
