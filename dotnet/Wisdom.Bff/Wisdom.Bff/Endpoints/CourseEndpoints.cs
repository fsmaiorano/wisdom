using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Endpoints;

public static class CourseEndpoints
{
    public static IEndpointRouteBuilder MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses").WithTags("Courses");

        group.MapGet("", async (ICourseApiClient client, CancellationToken ct) =>
                Results.Ok(await client.ListCoursesAsync(ct)))
            .WithName("ListCourses")
            .AllowAnonymous();

        group.MapGet("/my", async (ICourseApiClient client, CancellationToken ct) =>
                Results.Ok(await client.GetMyCoursesAsync(ct)))
            .WithName("GetMyCourses")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, ICourseApiClient client, CancellationToken ct) =>
            {
                var course = await client.GetCourseAsync(id, ct);
                return course is null ? Results.NotFound() : Results.Ok(course);
            })
            .WithName("GetCourse")
            .AllowAnonymous();

        group.MapPost("", async (CreateCourseRequest request, ICourseApiClient client, CancellationToken ct) =>
            {
                var result = await client.CreateCourseAsync(request, ct);
                return Results.Created($"/api/courses/{result.CourseId}", result);
            })
            .WithName("CreateCourse")
            .RequireAuthorization();

        group.MapPut("/{id:guid}", async (Guid id, UpdateCourseRequest request, ICourseApiClient client, CancellationToken ct) =>
                Results.Ok(await client.UpdateCourseAsync(id, request, ct)))
            .WithName("UpdateCourse")
            .RequireAuthorization();

        group.MapPatch("/{id:guid}/publish", async (Guid id, PublishCourseRequest request, ICourseApiClient client, CancellationToken ct) =>
                Results.Ok(await client.PublishCourseAsync(id, request, ct)))
            .WithName("PublishCourse")
            .RequireAuthorization();

        return app;
    }
}
