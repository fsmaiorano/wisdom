using System.Net.Http.Json;

namespace Wisdom.Bff.Clients;

public interface ILearningApiClient
{
    Task<IReadOnlyList<BackendProgress>> GetCourseProgressAsync(
        IReadOnlyList<Guid> lessonIds, CancellationToken ct);

    Task<BackendProgress> UpdateProgressAsync(UpdateProgressRequest request, CancellationToken ct);
    Task<BackendProgress> GetProgressByLessonAsync(Guid lessonId, CancellationToken ct);
}

// Mirrors Wisdom.Learning.Application.Dtos
public record BackendProgress(
    Guid ProgressId,
    Guid UserId,
    Guid LessonId,
    int WatchedSeconds,
    bool IsCompleted,
    DateTime? UpdatedAt);

public record UpdateProgressRequest(Guid LessonId, int WatchedSeconds, bool IsCompleted);
public record GetCourseProgressRequest(IReadOnlyList<Guid> LessonIds);

public sealed class LearningApiClient(HttpClient httpClient) : DownstreamClientBase(httpClient), ILearningApiClient
{
    public async Task<IReadOnlyList<BackendProgress>> GetCourseProgressAsync(
        IReadOnlyList<Guid> lessonIds, CancellationToken ct)
    {
        // Not an error case — nothing to look up, so skip the call entirely.
        if (lessonIds.Count == 0)
            return [];

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/progress/course")
        {
            Content = JsonContent.Create(new GetCourseProgressRequest(lessonIds), options: JsonOptions),
        };
        return await SendAsync<List<BackendProgress>>(req, ct) ?? [];
    }

    public Task<BackendProgress> UpdateProgressAsync(UpdateProgressRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/progress")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        return SendAsync<BackendProgress>(req, ct)!;
    }

    public Task<BackendProgress> GetProgressByLessonAsync(Guid lessonId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/progress/lesson/{lessonId}");
        return SendAsync<BackendProgress>(req, ct)!;
    }
}
