using System.Net.Http.Json;

namespace Wisdom.Bff.Clients;

public interface ITagApiClient
{
    Task<IReadOnlyList<BackendTag>> GetTagsAsync(CancellationToken ct);
    Task<BackendTag> CreateTagAsync(CreateTagRequest request, CancellationToken ct);
    Task AddTagToCourseAsync(Guid courseId, Guid tagId, CancellationToken ct);
}

public record BackendTag(Guid TagId, string Name);
public record CreateTagRequest(string Name);

public sealed class TagApiClient(HttpClient httpClient) : DownstreamClientBase(httpClient), ITagApiClient
{
    public async Task<IReadOnlyList<BackendTag>> GetTagsAsync(CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/tags");
        return await SendAsync<List<BackendTag>>(req, ct) ?? [];
    }

    public Task<BackendTag> CreateTagAsync(CreateTagRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/tags")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        return SendAsync<BackendTag>(req, ct)!;
    }

    // Note: this is genuinely nested under /api/tags on the Course service, not /api/courses.
    public Task AddTagToCourseAsync(Guid courseId, Guid tagId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, $"/api/tags/courses/{courseId}/tags/{tagId}");
        return SendAsync(req, ct);
    }
}
