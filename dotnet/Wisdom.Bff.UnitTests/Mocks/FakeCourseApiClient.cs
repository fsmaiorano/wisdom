using Wisdom.Bff.Clients;

namespace Wisdom.Bff.UnitTests.Mocks;

/// <summary>
/// Supports per-courseId setup via <see cref="Setup(Guid,BackendCourse?,System.Collections.Generic.List{BackendLesson}?)"/>
/// and a fallback via <see cref="Setup(BackendCourse?,System.Collections.Generic.List{BackendLesson}?)"/>
/// for tests that only deal with a single course.
/// </summary>
public sealed class FakeCourseApiClient : ICourseApiClient
{
    private readonly Dictionary<Guid, BackendCourse?> _courses = [];
    private readonly Dictionary<Guid, List<BackendLesson>> _lessons = [];
    private BackendCourse? _fallbackCourse;
    private List<BackendLesson> _fallbackLessons = [];

    public void Setup(BackendCourse? course, List<BackendLesson>? lessons = null)
    {
        _fallbackCourse = course;
        _fallbackLessons = lessons ?? [];
    }

    public void Setup(Guid courseId, BackendCourse? course, List<BackendLesson>? lessons = null)
    {
        _courses[courseId] = course;
        if (lessons is not null) _lessons[courseId] = lessons;
    }

    public Task<BackendCourse?> GetCourseAsync(Guid courseId, CancellationToken ct)
        => Task.FromResult(_courses.TryGetValue(courseId, out var c) ? c : _fallbackCourse);

    public Task<IReadOnlyList<BackendLesson>> GetLessonsAsync(Guid courseId, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<BackendLesson>>(
            _lessons.TryGetValue(courseId, out var l) ? l : _fallbackLessons);
}
