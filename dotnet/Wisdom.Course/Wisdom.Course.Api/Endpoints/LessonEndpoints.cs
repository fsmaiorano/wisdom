using Microsoft.AspNetCore.Mvc;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Application.Interfaces;
using Wisdom.Course.Application.UseCases;

namespace Wisdom.Course.Api.Endpoints;

public static class LessonEndpoints
{
    public static IEndpointRouteBuilder MapLessonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses/{courseId:guid}/lessons").WithTags("Lessons");

        group.MapGet("/", async (Guid courseId, GetLessonsUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(courseId, ct);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetLessons")
        .Produces<IReadOnlyList<GetLessonResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/", async (
                Guid courseId,
                [FromForm] string title,
                [FromForm] string description,
                [FromForm] int position,
                [FromForm] bool isPreview,
                [FromForm] string? thumbnailUrl,
                IFormFile? video,
                [FromServices] CreateLessonUseCase createLessonUseCase,
                [FromServices] IMediaApiClient mediaApiClient,
                CancellationToken ct) =>
            {
                var lessonResult = await createLessonUseCase.ExecuteAsync(
                    courseId, new CreateLessonRequest(title, description, position, isPreview), ct);

                if (!lessonResult.IsSuccess)
                    return Results.BadRequest(new { error = lessonResult.Error });

                var lesson = lessonResult.Data!;

                if (video is not null)
                {
                    await using var stream = video.OpenReadStream();
                    var mediaResult = await mediaApiClient.UploadAsync(
                        lesson.LessonId, "Video", stream, video.FileName, video.ContentType, ct);

                    if (mediaResult.IsSuccess)
                        lesson = lesson with { MediaId = mediaResult.Data!.MediaId };
                }

                return Results.Created($"/api/courses/{courseId}/lessons/{lesson.LessonId}", lesson);
            })
        .WithName("CreateLesson")
        .RequireAuthorization()
        .DisableAntiforgery()
        .Produces<GetLessonResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{lessonId:guid}", async (Guid courseId, Guid lessonId, UpdateLessonRequest request, UpdateLessonUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(lessonId, request, ct);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("UpdateLesson")
        .RequireAuthorization()
        .Produces<GetLessonResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/{lessonId:guid}", async (Guid courseId, Guid lessonId, DeleteLessonUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(lessonId, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("DeleteLesson")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
