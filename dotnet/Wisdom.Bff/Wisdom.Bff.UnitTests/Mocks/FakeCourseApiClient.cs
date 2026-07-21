using Wisdom.Bff.Clients;

namespace Wisdom.Bff.UnitTests.Mocks;

/// <summary>
/// Supports per-courseId setup via <see cref="Setup(Guid,BackendCourse?)"/>
/// and a fallback via <see cref="Setup(BackendCourse?)"/> for tests that only
/// deal with a single course. Lessons live on <see cref="FakeLessonApiClient"/>.
/// </summary>
public sealed class FakeCourseApiClient : ICourseApiClient
{
    private readonly Dictionary<Guid, BackendCourse?> _courses = [];
    private BackendCourse? _fallbackCourse;

    public void Setup(BackendCourse? course) => _fallbackCourse = course;

    public void Setup(Guid courseId, BackendCourse? course) => _courses[courseId] = course;

    public Task<BackendCourse?> GetCourseAsync(Guid courseId, CancellationToken ct)
        => Task.FromResult(_courses.TryGetValue(courseId, out var c) ? c : _fallbackCourse);

    // Not exercised by CoursePageHandler/DashboardHandler tests — only GetCourseAsync
    // above is. Stubs keep the fake compiling against the full interface.
    public Task<IReadOnlyList<BackendCourse>> ListCoursesAsync(CancellationToken ct) => throw new NotImplementedException();
    public Task<IReadOnlyList<BackendCourse>> GetMyCoursesAsync(CancellationToken ct) => throw new NotImplementedException();
    public Task<BackendCourse> CreateCourseAsync(CreateCourseRequest request, CancellationToken ct) => throw new NotImplementedException();
    public Task<BackendCourse> UpdateCourseAsync(Guid courseId, UpdateCourseRequest request, CancellationToken ct) => throw new NotImplementedException();
    public Task<BackendCourse> PublishCourseAsync(Guid courseId, PublishCourseRequest request, CancellationToken ct) => throw new NotImplementedException();
}
