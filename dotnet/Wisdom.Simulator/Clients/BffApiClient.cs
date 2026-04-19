using System.Net.Http.Headers;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Clients;

/// <summary>
/// Single HTTP client for all interactions routed through the BFF.
/// Course/Order/Learning calls go via YARP proxy (/api/*).
/// Dashboard and CoursePage calls hit BFF aggregation endpoints (/bff/*).
/// </summary>
public class BffApiClient(string baseUrl) : ApiClientBase(baseUrl)
{
    // ── Auth (proxied to User service) ─────────────────────────────────────

    public Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
        => PostAsync<RegisterUserResponse>("/api/auth/register", request, ct);

    public Task<LoginUserResponse?> LoginAsync(LoginUserRequest request, CancellationToken ct = default)
        => PostAsync<LoginUserResponse>("/api/auth/login", request, ct);

    // ── Courses (proxied to Course service) ────────────────────────────────

    public Task<CreateCourseResponse?> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct = default)
        => PostAsync<CreateCourseResponse>("/api/courses", request, ct);

    public async Task<(GetLessonResponse? Lesson, string? UploadError)> CreateLessonAsync(
        Guid courseId, CreateLessonRequest request, string? videoFilePath = null, string? thumbnailUrl = null, CancellationToken ct = default)
    {
        var lesson = await PostAsyncWithMultipartFallback<GetLessonResponse>($"/api/courses/{courseId}/lessons", request, ct);

        if (lesson is null || videoFilePath is null || !File.Exists(videoFilePath))
            return (lesson, null);

        try
        {
            await using var fileStream = File.OpenRead(videoFilePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/x-matroska");

            var form = new MultipartFormDataContent();
            form.Add(fileContent, "file", Path.GetFileName(videoFilePath));

            await PostMultipartAsync<object>($"/api/media/upload?lessonId={lesson.LessonId}&courseId={courseId}&type=Video", form, ct);
            return (lesson, null);
        }
        catch (HttpRequestException ex)
        {
            return (lesson, ex.Message);
        }
    }

    public Task<GetCourseResponse?> PublishCourseAsync(Guid courseId, bool publish, CancellationToken ct = default)
        => PatchAsync<GetCourseResponse>($"/api/courses/{courseId}/publish", new PublishCourseRequest(publish), ct);

    // ── Orders (proxied to Order service) ──────────────────────────────────

    public Task<PurchaseCourseResponse?> PurchaseAsync(PurchaseCourseRequest request, CancellationToken ct = default)
        => PostAsync<PurchaseCourseResponse>("/api/orders/purchase", request, ct);

    // ── Learning (proxied to Learning service) ─────────────────────────────

    public Task<GetProgressResponse?> UpdateProgressAsync(UpdateProgressRequest request, CancellationToken ct = default)
        => PostAsync<GetProgressResponse>("/api/progress", request, ct);

    // ── BFF aggregation endpoints ───────────────────────────────────────────

    public Task<DashboardResponse?> GetDashboardAsync(CancellationToken ct = default)
        => GetAsync<DashboardResponse>("/bff/dashboard", ct);

    public Task<CoursePageResponse?> GetCoursePageAsync(Guid courseId, CancellationToken ct = default)
        => GetAsync<CoursePageResponse>($"/bff/courses/{courseId}/page", ct);
}
