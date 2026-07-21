using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags").WithTags("Tags");

        group.MapGet("", async (ITagApiClient client, CancellationToken ct) =>
                Results.Ok(await client.GetTagsAsync(ct)))
            .WithName("GetTags")
            .AllowAnonymous();

        group.MapPost("", async (CreateTagRequest request, ITagApiClient client, CancellationToken ct) =>
            {
                var result = await client.CreateTagAsync(request, ct);
                return Results.Created($"/api/tags/{result.TagId}", result);
            })
            .WithName("CreateTag")
            .RequireAuthorization();

        // Genuinely nested under /api/tags on the downstream Course service, not /api/courses.
        group.MapPost("/courses/{courseId:guid}/tags/{tagId:guid}", async (Guid courseId, Guid tagId, ITagApiClient client, CancellationToken ct) =>
            {
                await client.AddTagToCourseAsync(courseId, tagId, ct);
                return Results.NoContent();
            })
            .WithName("AddTagToCourse")
            .RequireAuthorization();

        return app;
    }
}
