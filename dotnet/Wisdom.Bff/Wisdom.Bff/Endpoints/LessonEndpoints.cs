using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Endpoints;

public static class LessonEndpoints
{
    public static IEndpointRouteBuilder MapLessonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses/{courseId:guid}/lessons").WithTags("Lessons");

        group.MapGet("", async (Guid courseId, ILessonApiClient client, CancellationToken ct) =>
                Results.Ok(await client.GetLessonsAsync(courseId, ct)))
            .WithName("GetLessons")
            .AllowAnonymous();

        // Downstream 404s (with its own { error } body) are relayed as-is by DownstreamExceptionHandler.
        group.MapGet("/{lessonId:guid}", async (Guid courseId, Guid lessonId, ILessonApiClient client, CancellationToken ct) =>
                Results.Ok(await client.GetLessonAsync(courseId, lessonId, ct)))
            .WithName("GetLesson")
            .AllowAnonymous();

        group.MapPost("", async (Guid courseId, CreateLessonRequest request, ILessonApiClient client, CancellationToken ct) =>
            {
                var result = await client.CreateLessonAsync(courseId, request, ct);
                return Results.Created($"/api/courses/{courseId}/lessons/{result.LessonId}", result);
            })
            .WithName("CreateLesson")
            .RequireAuthorization();

        group.MapPut("/{lessonId:guid}", async (Guid courseId, Guid lessonId, UpdateLessonRequest request, ILessonApiClient client, CancellationToken ct) =>
                Results.Ok(await client.UpdateLessonAsync(courseId, lessonId, request, ct)))
            .WithName("UpdateLesson")
            .RequireAuthorization();

        group.MapDelete("/{lessonId:guid}", async (Guid courseId, Guid lessonId, ILessonApiClient client, CancellationToken ct) =>
            {
                await client.DeleteLessonAsync(courseId, lessonId, ct);
                return Results.NoContent();
            })
            .WithName("DeleteLesson")
            .RequireAuthorization();

        return app;
    }
}
