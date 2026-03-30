using System.Net.Http.Headers;
using System.Net.Http.Json;
using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Application.Interfaces;

namespace Wisdom.Course.Infrastructure.Services;

public sealed class MediaApiClient(HttpClient httpClient) : IMediaApiClient
{
    public async Task<Result<UploadMediaResponse>> UploadAsync(
        Guid lessonId,
        string mediaType,
        Stream fileContent,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileContent);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "file", fileName);

            var response = await httpClient.PostAsync(
                $"/api/media/upload?lessonId={lessonId}&type={mediaType}", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                return Result<UploadMediaResponse>.Failure($"Media upload failed: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<UploadMediaApiResponse>(cancellationToken: ct);
            if (result is null)
                return Result<UploadMediaResponse>.Failure("Invalid response from media service.");

            return Result<UploadMediaResponse>.Success(new UploadMediaResponse(result.MediaId));
        }
        catch (HttpRequestException ex)
        {
            return Result<UploadMediaResponse>.Failure($"Media service unavailable: {ex.Message}");
        }
    }

    private sealed record UploadMediaApiResponse(Guid MediaId);
}
