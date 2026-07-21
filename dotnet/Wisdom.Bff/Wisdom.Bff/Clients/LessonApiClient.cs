using System.Net.Http.Json;

namespace Wisdom.Bff.Clients;

public interface ILessonApiClient
{
    Task<IReadOnlyList<BackendLesson>> GetLessonsAsync(Guid courseId, CancellationToken ct);
    Task<BackendLesson?> GetLessonAsync(Guid courseId, Guid lessonId, CancellationToken ct);
    Task<BackendLesson> CreateLessonAsync(Guid courseId, CreateLessonRequest request, CancellationToken ct);
    Task<BackendLesson> UpdateLessonAsync(Guid courseId, Guid lessonId, UpdateLessonRequest request, CancellationToken ct);
    Task DeleteLessonAsync(Guid courseId, Guid lessonId, CancellationToken ct);
}

// Mirrors Wisdom.Course.Application.Dtos
public record BackendLesson(
    Guid LessonId,
    Guid CourseId,
    string Title,
    string Description,
    int Position,
    bool IsPreview,
    DateTime? CreatedAt,
    Guid? MediaId);

public record CreateLessonRequest(string Title, string Description, int Position, bool IsPreview);
public record UpdateLessonRequest(string Title, string Description, int Position, bool IsPreview);

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
