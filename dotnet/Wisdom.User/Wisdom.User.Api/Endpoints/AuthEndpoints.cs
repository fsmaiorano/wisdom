using System.Security.Claims;
using Wisdom.BuildingBlocks.Extensions;
using Wisdom.User.Application.Dtos;
using Wisdom.User.Application.UseCases;
namespace Wisdom.User.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").AllowAnonymous();

        group.MapPost("/register", async (ClaimsPrincipal user, RegisterUserRequest request, RegisterUserUseCase useCase, CancellationToken ct) =>
        {
            var actor = user.GetEmail() ?? "system";
            var result = await useCase.ExecuteAsync(request, actor, ct);
            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Data!.UserId}", result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("RegisterUser")
        .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/login", async (LoginUserRequest request, LoginUserUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("LoginUser")
        .Produces<LoginUserResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/refresh", async (RefreshTokenRequest request, RefreshTokenUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("RefreshToken")
        .Produces<RefreshTokenResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/logout", async (LogoutUserRequest request, LogoutUserUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(request, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("LogoutUser")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
