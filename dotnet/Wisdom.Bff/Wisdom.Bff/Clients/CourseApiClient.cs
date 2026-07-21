using System.Net.Http.Json;

namespace Wisdom.Bff.Clients;

public interface ICourseApiClient
{
    Task<BackendCourse?> GetCourseAsync(Guid courseId, CancellationToken ct);

    Task<IReadOnlyList<BackendCourse>> ListCoursesAsync(CancellationToken ct);
    Task<IReadOnlyList<BackendCourse>> GetMyCoursesAsync(CancellationToken ct);
    Task<BackendCourse> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct);
    Task<BackendCourse> UpdateCourseAsync(Guid courseId, UpdateCourseRequest request, CancellationToken ct);
    Task<BackendCourse> PublishCourseAsync(Guid courseId, PublishCourseRequest request, CancellationToken ct);
}

// Mirrors Wisdom.Course.Application.Dtos
public record BackendCourse(
    Guid CourseId,
    Guid TeacherId,
    string Title,
    string Description,
    decimal Price,
    bool IsPublished,
    DateTime? CreatedAt);

public record CreateCourseRequest(string Title, string Description, decimal Price);
public record UpdateCourseRequest(string Title, string Description, decimal Price);
public record PublishCourseRequest(bool Publish);

public sealed class CourseApiClient(HttpClient httpClient) : DownstreamClientBase(httpClient), ICourseApiClient
{
    public Task<BackendCourse?> GetCourseAsync(Guid courseId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/courses/{courseId}");
        return SendOrDefaultAsync<BackendCourse>(req, ct);
    }

    public async Task<IReadOnlyList<BackendCourse>> ListCoursesAsync(CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/courses");
        return await SendAsync<List<BackendCourse>>(req, ct) ?? [];
    }

    public async Task<IReadOnlyList<BackendCourse>> GetMyCoursesAsync(CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "/api/courses/my");
        return await SendAsync<List<BackendCourse>>(req, ct) ?? [];
    }

    public Task<BackendCourse> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/courses")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        return SendAsync<BackendCourse>(req, ct)!;
    }

    public Task<BackendCourse> UpdateCourseAsync(Guid courseId, UpdateCourseRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, $"/api/courses/{courseId}")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        return SendAsync<BackendCourse>(req, ct)!;
    }

    public Task<BackendCourse> PublishCourseAsync(Guid courseId, PublishCourseRequest request, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, $"/api/courses/{courseId}/publish")
        {
            Content = JsonContent.Create(request, options: JsonOptions),
        };
        return SendAsync<BackendCourse>(req, ct)!;
    }
}
