namespace Wisdom.Bff.Clients;

public interface ICourseApiClient
{
    Task<BackendCourse?> GetCourseAsync(Guid courseId, CancellationToken ct);
    Task<IReadOnlyList<BackendLesson>> GetLessonsAsync(Guid courseId, CancellationToken ct);
}

public interface IOrderApiClient
{
    Task<BackendCheckEnrollment?> CheckEnrollmentAsync(Guid courseId, CancellationToken ct);
    Task<IReadOnlyList<BackendEnrollment>> GetEnrollmentsAsync(CancellationToken ct);
}

public interface ILearningApiClient
{
    Task<IReadOnlyList<BackendProgress>> GetCourseProgressAsync(
        IReadOnlyList<Guid> lessonIds, CancellationToken ct);
}
