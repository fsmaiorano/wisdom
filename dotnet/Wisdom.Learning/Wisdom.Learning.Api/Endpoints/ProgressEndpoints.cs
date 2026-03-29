using System.Security.Claims;
using Wisdom.BuildingBlocks.Extensions;
using Wisdom.Learning.Application.Dtos;
using Wisdom.Learning.Application.UseCases;
namespace Wisdom.Learning.Api.Endpoints;

public static class ProgressEndpoints
{
    public static IEndpointRouteBuilder MapProgressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/progress").WithTags("Progress").RequireAuthorization();

        group.MapPost("/", async (ClaimsPrincipal user, UpdateProgressRequest request, UpdateProgressUseCase useCase, CancellationToken ct) =>
            {
                var userId = user.GetUserId();
                if (userId is null) return Results.Unauthorized();
                var result = await useCase.ExecuteAsync(userId.Value, request, ct);
                return result.IsSuccess
                    ? Results.Ok(result.Data)
                    : Results.BadRequest(result.Error);
            })
            .WithName("UpdateProgress")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/lesson/{lessonId:guid}", async (ClaimsPrincipal user, Guid lessonId, GetProgressUseCase useCase, CancellationToken ct) =>
            {
                var userId = user.GetUserId();
                if (userId is null) return Results.Unauthorized();
                var result = await useCase.ExecuteAsync(userId.Value, lessonId, ct);
                return result.IsSuccess
                    ? Results.Ok(result.Data)
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetProgressByLesson")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/course", async (ClaimsPrincipal user, GetCourseProgressRequest request, GetCourseProgressUseCase useCase, CancellationToken ct) =>
            {
                var userId = user.GetUserId();
                if (userId is null) return Results.Unauthorized();
                var result = await useCase.ExecuteAsync(userId.Value, request, ct);
                return result.IsSuccess
                    ? Results.Ok(result.Data)
                    : Results.BadRequest(result.Error);
            })
            .WithName("GetCourseProgress")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
