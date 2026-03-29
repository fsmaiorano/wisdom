using System.Security.Claims;
using Wisdom.BuildingBlocks.Extensions;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Application.UseCases;
namespace Wisdom.Course.Api.Endpoints;

public static class CourseEndpoints
{
    public static IEndpointRouteBuilder MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses").WithTags("Courses");

        group.MapGet("/", async (ListCoursesUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(ct);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("ListCourses")
        .Produces<IReadOnlyList<GetCourseResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", async (Guid id, GetCourseUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(id, ct);
            if (!result.IsSuccess)
                return Results.BadRequest(new { error = result.Error });
            return result.Data is null
                ? Results.NotFound()
                : Results.Ok(result.Data);
        })
        .WithName("GetCourse")
        .Produces<GetCourseResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/", async (ClaimsPrincipal user, CreateCourseRequest request, CreateCourseUseCase useCase, CancellationToken ct) =>
        {
            var teacherId = user.GetUserId();
            if (teacherId is null) return Results.Unauthorized();
            var result = await useCase.ExecuteAsync(teacherId.Value, request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/courses/{result.Data!.CourseId}", result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateCourse")
        .RequireAuthorization()
        .Produces<CreateCourseResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateCourseRequest request, UpdateCourseUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(id, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UpdateCourse")
        .RequireAuthorization()
        .Produces<GetCourseResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/{id:guid}/publish", async (Guid id, PublishCourseRequest request, PublishCourseUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(id, request.Publish, ct);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("PublishCourse")
        .RequireAuthorization()
        .Produces<GetCourseResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}

public record PublishCourseRequest(bool Publish);
