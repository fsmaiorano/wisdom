using System.Security.Claims;
using Wisdom.User.Application.UseCases;
namespace Wisdom.User.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/me", async (HttpContext context, MeUseCase useCase, CancellationToken ct) =>
            {
                var email = context.User.FindFirst("email")?.Value
                            ?? context.User.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                    return Results.Unauthorized();

                var userEntity = await useCase.ExecuteAsync(email, ct);

                return Results.Ok(new
                {
                    UserId = userEntity.Data?.Id.Value,
                    KeycloakId = userEntity.Data?.KeycloakId,
                    Email = email,
                });
            })
            .WithName("GetCurrentUser")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}