using Wisdom.Bff.Clients;

namespace Wisdom.Bff.UnitTests.Mocks;

/// <summary>
/// Supports per-courseId setup via <see cref="Setup(Guid,List{BackendLesson}?)"/>
/// and a fallback via <see cref="Setup(List{BackendLesson}?)"/> for tests that
/// only deal with a single course.
/// </summary>
public sealed class FakeLessonApiClient : ILessonApiClient
{
    private readonly Dictionary<Guid, List<BackendLesson>> _lessons = [];
    private List<BackendLesson> _fallbackLessons = [];

    public void Setup(List<BackendLesson>? lessons = null) => _fallbackLessons = lessons ?? [];

    public void Setup(Guid courseId, List<BackendLesson>? lessons = null) => _lessons[courseId] = lessons ?? [];

    public Task<IReadOnlyList<BackendLesson>> GetLessonsAsync(Guid courseId, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<BackendLesson>>(
            _lessons.TryGetValue(courseId, out var l) ? l : _fallbackLessons);

    // Not exercised by CoursePageHandler/DashboardHandler tests — only GetLessonsAsync
    // above is. Stubs keep the fake compiling against the full interface.
    public Task<BackendLesson?> GetLessonAsync(Guid courseId, Guid lessonId, CancellationToken ct) => throw new NotImplementedException();
    public Task<BackendLesson> CreateLessonAsync(Guid courseId, CreateLessonRequest request, CancellationToken ct) => throw new NotImplementedException();
    public Task<BackendLesson> UpdateLessonAsync(Guid courseId, Guid lessonId, UpdateLessonRequest request, CancellationToken ct) => throw new NotImplementedException();
    public Task DeleteLessonAsync(Guid courseId, Guid lessonId, CancellationToken ct) => throw new NotImplementedException();
}
