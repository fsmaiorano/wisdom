namespace Wisdom.Bff.Aggregations.Courses;

public static class CoursePageEndpoint
{
    public static IEndpointRouteBuilder MapCoursePageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/bff/courses/{courseId:guid}/page", async (
            Guid courseId,
            HttpContext httpContext,
            CoursePageHandler handler,
            CancellationToken ct) =>
        {
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
            var result = await handler.ExecuteAsync(courseId, isAuthenticated, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetCoursePage")
        .WithSummary("Course detail page — aggregates course info, lessons, enrollment and progress in one call")
        .WithTags("BFF — Aggregations")
        .Produces<CoursePageResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
