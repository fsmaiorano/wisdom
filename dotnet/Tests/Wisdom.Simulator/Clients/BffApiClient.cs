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

    public async Task<GetLessonResponse?> CreateLessonAsync(
        Guid courseId, CreateLessonRequest request, CancellationToken ct = default)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(request.Title),                           "title");
        form.Add(new StringContent(request.Description),                     "description");
        form.Add(new StringContent(request.Position.ToString()),             "position");
        form.Add(new StringContent(request.IsPreview.ToString().ToLower()),  "isPreview");
        return await PostMultipartAsync<GetLessonResponse>($"/api/courses/{courseId}/lessons", form, ct);
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
