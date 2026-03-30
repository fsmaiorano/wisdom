using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Clients;

public class MediaApiClient(string baseUrl) : ApiClientBase(baseUrl)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Uploads a video file for the given lesson directly to the Media service via the BFF proxy.
    /// POST /api/media/upload?lessonId={lessonId}&amp;type=Video
    /// </summary>
    public async Task<GetMediaResponse?> UploadVideoAsync(
        Guid lessonId, string videoFilePath, CancellationToken ct = default)
    {
        if (!File.Exists(videoFilePath))
            return null;

        await using var fileStream = File.OpenRead(videoFilePath);
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("video/x-matroska");
        content.Add(streamContent, "file", Path.GetFileName(videoFilePath));

        var response = await Http.PostAsync(
            $"/api/media/upload?lessonId={lessonId}&type=Video", content, ct);

        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<GetMediaResponse>(JsonOptions, ct);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException(
            $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} — {body}");
    }
}
