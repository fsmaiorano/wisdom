using Microsoft.Extensions.Options;
using Wisdom.Streaming.Application.Services;
using Wisdom.Streaming.Application.Settings;
namespace Wisdom.Streaming.Api.Endpoints;

public static class StreamEndpoints
{
    public static IEndpointRouteBuilder MapStreamEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok()).WithTags("Health");

        var disableAuth = app.ServiceProvider.GetRequiredService<IOptions<StreamingSettings>>().Value.DisableAuth;

        var stream = app.MapMethods("/stream/{mediaId:guid}", ["GET", "HEAD"], StreamAsync)
            .WithTags("Stream")
            .WithName("StreamMedia")
            .Produces(StatusCodes.Status200OK, contentType: "video/mp4")
            .Produces(StatusCodes.Status206PartialContent, contentType: "video/mp4")
            .ProducesProblem(StatusCodes.Status404NotFound);

        if (!disableAuth)
            stream.RequireAuthorization();

        return app;
    }

    private static async Task<IResult> StreamAsync(
        Guid mediaId,
        string? quality,
        HttpRequest request,
        HttpResponse response,
        StreamingService service,
        Wisdom.Streaming.Application.Abstractions.IVideoStorage storage,
        CancellationToken ct)
    {
        var lookup = await service.ResolveAsync(mediaId, quality, ct);

        switch (lookup.Status)
        {
            case StreamLookupStatus.InvalidQuality:
                return Results.BadRequest(new { error = "invalid quality: use 480p, 720p, or 1080p" });
            case StreamLookupStatus.MediaNotFound:
                return Results.NotFound(new { error = "media not found" });
            case StreamLookupStatus.MediaNotReady:
                return Results.NotFound(new { error = $"media not ready (status={lookup.MediaStatus})" });
            case StreamLookupStatus.FileNotFound:
                return Results.NotFound(new { error = "video file not found" });
        }

        response.ContentType = "video/mp4";
        response.Headers.AcceptRanges = "bytes";
        response.Headers.CacheControl = "no-cache";

        if (HttpMethods.IsHead(request.Method))
        {
            response.ContentLength = lookup.Size;
            return Results.Empty;
        }

        var rangeHeader = request.Headers.Range.ToString();
        ByteRange range;

        if (string.IsNullOrEmpty(rangeHeader))
        {
            range = new ByteRange(0, lookup.Size - 1);
            response.StatusCode = StatusCodes.Status200OK;
        }
        else if (ByteRange.TryParse(rangeHeader, lookup.Size, out range))
        {
            response.StatusCode = StatusCodes.Status206PartialContent;
            response.Headers.ContentRange = $"bytes {range.Start}-{range.End}/{lookup.Size}";
        }
        else
        {
            response.Headers.ContentRange = $"bytes */{lookup.Size}";
            return Results.StatusCode(StatusCodes.Status416RangeNotSatisfiable);
        }

        response.ContentLength = range.Length;

        if (range.Length > 0)
            await storage.CopyRangeAsync(lookup.ObjectPath, range.Start, range.Length, response.Body, ct);

        return Results.Empty;
    }
}
