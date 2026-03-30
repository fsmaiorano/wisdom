using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Clients;

public class CourseApiClient(string baseUrl) : ApiClientBase(baseUrl)
{
    public Task<CreateCourseResponse?> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct = default)
        => PostAsync<CreateCourseResponse>("/api/courses", request, ct);

    public Task<GetLessonResponse?> CreateLessonAsync(Guid courseId, CreateLessonRequest request, CancellationToken ct = default)
        => PostAsync<GetLessonResponse>($"/api/courses/{courseId}/lessons", request, ct);

    public Task<GetCourseResponse?> PublishCourseAsync(Guid courseId, bool publish, CancellationToken ct = default)
        => PatchAsync<GetCourseResponse>($"/api/courses/{courseId}/publish", new PublishCourseRequest(publish), ct);

    public Task<List<GetCourseResponse>?> ListCoursesAsync(CancellationToken ct = default)
        => GetAsync<List<GetCourseResponse>>("/api/courses", ct);

    public Task<List<GetLessonResponse>?> GetLessonsAsync(Guid courseId, CancellationToken ct = default)
        => GetAsync<List<GetLessonResponse>>($"/api/courses/{courseId}/lessons", ct);
}
