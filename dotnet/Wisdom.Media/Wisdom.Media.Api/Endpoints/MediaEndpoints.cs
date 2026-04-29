using Microsoft.AspNetCore.Mvc;
using Wisdom.Media.Application.Dtos;
using Wisdom.Media.Application.UseCases;
using Wisdom.Media.Domain.Enums;

namespace Wisdom.Media.Api.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media").WithTags("Media");

        group.MapGet("/{id:guid}", async (Guid id, GetMediaUseCase useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(id, ct);
                return result.IsSuccess
                    ? Results.Ok(result.Data)
                    : Results.NotFound(new { error = result.Error });
            })
            .WithName("GetMedia")
            .Produces<GetMediaResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/lesson/{lessonId:guid}",
                async (Guid lessonId, GetLessonMediaUseCase useCase, CancellationToken ct) =>
                {
                    var result = await useCase.ExecuteAsync(lessonId, ct);
                    return Results.Ok(result.Data);
                })
            .WithName("GetLessonMedia")
            .Produces<IReadOnlyList<GetMediaResponse>>(StatusCodes.Status200OK);

        group.MapPatch("/{id:guid}/status", async (
                Guid id,
                UpdateMediaStatusRequest request,
                UpdateMediaStatusUseCase useCase,
                CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(id, request, ct);
                return result.IsSuccess
                    ? Results.Ok(result.Data)
                    : Results.BadRequest(new { error = result.Error });
            })
            .RequireAuthorization()
            .WithName("UpdateMediaStatus")
            .Produces<GetMediaResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", async (Guid id, DeleteMediaUseCase useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(id, ct);
                return result.IsSuccess
                    ? Results.NoContent()
                    : Results.NotFound(new { error = result.Error });
            })
            .RequireAuthorization()
            .WithName("DeleteMedia")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/upload", async (
                [FromQuery] Guid lessonId,
                [FromQuery] Guid courseId,
                [FromQuery] string type,
                IFormFile file,
                UploadMediaUseCase useCase,
                CancellationToken ct) =>
            {
                if (!Enum.TryParse<MediaTypeEnum>(type, ignoreCase: true, out var mediaType))
                    return Results.BadRequest(new { error = $"Invalid media type '{type}'." });

                await using var stream = file.OpenReadStream();
                var result =
                    await useCase.ExecuteAsync(lessonId, courseId, mediaType, stream, file.FileName, file.ContentType, ct: ct);
                return result.IsSuccess
                    ? Results.Created($"/api/media/{result.Data!.MediaId}", result.Data)
                    : Results.BadRequest(new { error = result.Error });
            })
            .RequireAuthorization()
            .DisableAntiforgery()
            .WithName("UploadMedia")
            .Produces<GetMediaResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}