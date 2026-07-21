using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/me", async (IUserApiClient client, CancellationToken ct) =>
            Results.Ok(await client.GetMeAsync(ct)))
        .WithName("GetMe")
        .WithTags("Users")
        .RequireAuthorization()
        .Produces<BackendMe>(StatusCodes.Status200OK);

        return app;
    }
}
