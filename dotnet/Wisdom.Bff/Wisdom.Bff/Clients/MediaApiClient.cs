using System.Net.Http.Json;
using System.Web;

namespace Wisdom.Bff.Clients;

public sealed class MediaApiClient(HttpClient httpClient) : DownstreamClientBase(httpClient), IMediaApiClient
{
    public Task<BackendMedia?> GetMediaAsync(Guid mediaId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/media/{mediaId}");
        return SendAsync<BackendMedia>(req, ct);
    }

    public async Task<IReadOnlyList<BackendMedia>> GetLessonMediaAsync(Guid lessonId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/media/lesson/{lessonId}");
        return await SendAsync<List<BackendMedia>>(req, ct) ?? [];
    }

    public Task<BackendMedia> UploadMediaAsync(
        Guid lessonId, Guid courseId, string type, Stream fileStream, string fileName,
        string? contentType, CancellationToken ct)
    {
        var query = $"lessonId={lessonId}&courseId={courseId}&type={HttpUtility.UrlEncode(type)}";

        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        if (contentType is not null)
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var req = new HttpRequestMessage(HttpMethod.Post, $"/api/media/upload?{query}") { Content = content };
        return SendAsync<BackendMedia>(req, ct)!;
    }

    public Task<BackendMedia> UpdateMediaStatusAsync(Guid mediaId, UpdateMediaStatusRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, $"/api/media/{mediaId}/status")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        return SendAsync<BackendMedia>(req, ct)!;
    }

    public Task DeleteMediaAsync(Guid mediaId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/media/{mediaId}");
        return SendAsync(req, ct);
    }
}
