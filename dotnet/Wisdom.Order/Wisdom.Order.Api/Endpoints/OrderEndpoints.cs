using System.Security.Claims;
using Wisdom.BuildingBlocks.Extensions;
using Wisdom.Order.Application.Dtos;
using Wisdom.Order.Application.UseCases;
namespace Wisdom.Order.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization();

        group.MapPost("/purchase", async (ClaimsPrincipal user, PurchaseCourseRequest request, PurchaseCourseUseCase useCase, CancellationToken ct) =>
            {
                var userId = user.GetUserId();
                if (userId is null) return Results.Unauthorized();
                var result = await useCase.ExecuteAsync(userId.Value, request, ct);
                if (!result.IsSuccess)
                    return Results.BadRequest(result);
                return Results.Created($"/api/orders/purchase/{result.Data!.PurchaseId}", result.Data);
            })
            .WithName("PurchaseCourse")
            .Produces<PurchaseCourseResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/enrollments", async (ClaimsPrincipal user, GetEnrollmentsUseCase useCase, CancellationToken ct) =>
            {
                var userId = user.GetUserId();
                if (userId is null) return Results.Unauthorized();
                var result = await useCase.ExecuteAsync(userId.Value, ct);
                if (!result.IsSuccess)
                    return Results.BadRequest(result);
                return Results.Ok(result.Data);
            })
            .WithName("GetEnrollments")
            .Produces<IReadOnlyList<GetEnrollmentResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/enrollments/{courseId:guid}/check", async (ClaimsPrincipal user, Guid courseId, CheckEnrollmentUseCase useCase, CancellationToken ct) =>
            {
                var userId = user.GetUserId();
                if (userId is null) return Results.Unauthorized();
                var result = await useCase.ExecuteAsync(userId.Value, courseId, ct);
                if (!result.IsSuccess)
                    return Results.BadRequest(result);
                return Results.Ok(result.Data);
            })
            .WithName("CheckEnrollment")
            .Produces<CheckEnrollmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
