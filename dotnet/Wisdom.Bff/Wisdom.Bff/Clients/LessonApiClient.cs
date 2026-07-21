using System.Net.Http.Json;

namespace Wisdom.Bff.Clients;

public sealed class LessonApiClient(HttpClient httpClient) : DownstreamClientBase(httpClient), ILessonApiClient
{
    public async Task<IReadOnlyList<BackendLesson>> GetLessonsAsync(Guid courseId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/courses/{courseId}/lessons");
        return await SendAsync<List<BackendLesson>>(req, ct) ?? [];
    }

    public Task<BackendLesson?> GetLessonAsync(Guid courseId, Guid lessonId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/courses/{courseId}/lessons/{lessonId}");
        return SendAsync<BackendLesson>(req, ct);
    }

    public Task<BackendLesson> CreateLessonAsync(Guid courseId, CreateLessonRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, $"/api/courses/{courseId}/lessons")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        return SendAsync<BackendLesson>(req, ct)!;
    }

    public Task<BackendLesson> UpdateLessonAsync(Guid courseId, Guid lessonId, UpdateLessonRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, $"/api/courses/{courseId}/lessons/{lessonId}")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        return SendAsync<BackendLesson>(req, ct)!;
    }

    public Task DeleteLessonAsync(Guid courseId, Guid lessonId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, $"/api/courses/{courseId}/lessons/{lessonId}");
        return SendAsync(req, ct);
    }
}
