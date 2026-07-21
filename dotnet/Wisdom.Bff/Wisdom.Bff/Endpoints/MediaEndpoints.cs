using Microsoft.AspNetCore.Mvc;
using Wisdom.Bff.Clients;

namespace Wisdom.Bff.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media").WithTags("Media");

        group.MapGet("/{id:guid}", async (Guid id, IMediaApiClient client, CancellationToken ct) =>
                Results.Ok(await client.GetMediaAsync(id, ct)))
            .WithName("GetMedia")
            .AllowAnonymous();

        group.MapGet("/lesson/{lessonId:guid}", async (Guid lessonId, IMediaApiClient client, CancellationToken ct) =>
                Results.Ok(await client.GetLessonMediaAsync(lessonId, ct)))
            .WithName("GetLessonMedia")
            .AllowAnonymous();

        group.MapPost("/upload", async (
                [FromQuery] Guid lessonId,
                [FromQuery] Guid courseId,
                [FromQuery] string type,
                IFormFile file,
                IMediaApiClient client,
                CancellationToken ct) =>
            {
                await using var stream = file.OpenReadStream();
                var result = await client.UploadMediaAsync(
                    lessonId, courseId, type, stream, file.FileName, file.ContentType, ct);
                return Results.Created($"/api/media/{result.MediaId}", result);
            })
            .WithName("UploadMedia")
            .RequireAuthorization()
            .DisableAntiforgery();

        group.MapPatch("/{id:guid}/status", async (Guid id, UpdateMediaStatusRequest request, IMediaApiClient client, CancellationToken ct) =>
                Results.Ok(await client.UpdateMediaStatusAsync(id, request, ct)))
            .WithName("UpdateMediaStatus")
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", async (Guid id, IMediaApiClient client, CancellationToken ct) =>
            {
                await client.DeleteMediaAsync(id, ct);
                return Results.NoContent();
            })
            .WithName("DeleteMedia")
            .RequireAuthorization();

        return app;
    }
}
