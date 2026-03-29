using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Application.UseCases;
namespace Wisdom.Course.Api.Endpoints;

public static class TagEndpoints
{
    public static IEndpointRouteBuilder MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags").WithTags("Tags");

        group.MapGet("/", async (GetTagsUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(ct);
            return result.IsSuccess
                ? Results.Ok(result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("GetTags")
        .Produces<IReadOnlyList<GetTagResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/", async (CreateTagRequest request, CreateTagUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(request, ct);
            return result.IsSuccess
                ? Results.Created($"/api/tags/{result.Data!.TagId}", result.Data)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CreateTag")
        .RequireAuthorization()
        .Produces<GetTagResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/courses/{courseId:guid}/tags/{tagId:guid}", async (Guid courseId, Guid tagId, AddTagToCourseUseCase useCase, CancellationToken ct) =>
        {
            var result = await useCase.ExecuteAsync(courseId, tagId, ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("AddTagToCourse")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
