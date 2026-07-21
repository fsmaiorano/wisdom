using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Endpoints;

public static class ProgressEndpoints
{
    public static IEndpointRouteBuilder MapProgressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/progress").WithTags("Progress").RequireAuthorization();

        group.MapPost("", async (UpdateProgressRequest request, ILearningApiClient client, CancellationToken ct) =>
                Results.Ok(await client.UpdateProgressAsync(request, ct)))
            .WithName("UpdateProgress");

        group.MapGet("/lesson/{lessonId:guid}", async (Guid lessonId, ILearningApiClient client, CancellationToken ct) =>
                Results.Ok(await client.GetProgressByLessonAsync(lessonId, ct)))
            .WithName("GetProgressByLesson");

        group.MapPost("/course", async (GetCourseProgressRequest request, ILearningApiClient client, CancellationToken ct) =>
                Results.Ok(await client.GetCourseProgressAsync(request.LessonIds, ct)))
            .WithName("GetCourseProgress");

        return app;
    }
}
