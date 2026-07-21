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

public interface ILessonApiClient
{
    Task<IReadOnlyList<BackendLesson>> GetLessonsAsync(Guid courseId, CancellationToken ct);
    Task<BackendLesson?> GetLessonAsync(Guid courseId, Guid lessonId, CancellationToken ct);
    Task<BackendLesson> CreateLessonAsync(Guid courseId, CreateLessonRequest request, CancellationToken ct);
    Task<BackendLesson> UpdateLessonAsync(Guid courseId, Guid lessonId, UpdateLessonRequest request, CancellationToken ct);
    Task DeleteLessonAsync(Guid courseId, Guid lessonId, CancellationToken ct);
}

public interface ITagApiClient
{
    Task<IReadOnlyList<BackendTag>> GetTagsAsync(CancellationToken ct);
    Task<BackendTag> CreateTagAsync(CreateTagRequest request, CancellationToken ct);
    Task AddTagToCourseAsync(Guid courseId, Guid tagId, CancellationToken ct);
}

public interface IMediaApiClient
{
    Task<BackendMedia?> GetMediaAsync(Guid mediaId, CancellationToken ct);
    Task<IReadOnlyList<BackendMedia>> GetLessonMediaAsync(Guid lessonId, CancellationToken ct);
    Task<BackendMedia> UploadMediaAsync(
        Guid lessonId, Guid courseId, string type, Stream fileStream, string fileName,
        string? contentType, CancellationToken ct);
    Task<BackendMedia> UpdateMediaStatusAsync(Guid mediaId, UpdateMediaStatusRequest request, CancellationToken ct);
    Task DeleteMediaAsync(Guid mediaId, CancellationToken ct);
}

public interface IOrderApiClient
{
    Task<BackendCheckEnrollment> CheckEnrollmentAsync(Guid courseId, CancellationToken ct);
    Task<IReadOnlyList<BackendEnrollment>> GetEnrollmentsAsync(CancellationToken ct);

    Task<BackendPurchase> PurchaseCourseAsync(PurchaseCourseRequest request, CancellationToken ct);
}

public interface ILearningApiClient
{
    Task<IReadOnlyList<BackendProgress>> GetCourseProgressAsync(
        IReadOnlyList<Guid> lessonIds, CancellationToken ct);

    Task<BackendProgress> UpdateProgressAsync(UpdateProgressRequest request, CancellationToken ct);
    Task<BackendProgress> GetProgressByLessonAsync(Guid lessonId, CancellationToken ct);
}

public interface IUserApiClient
{
    Task<BackendRegisteredUser> RegisterAsync(RegisterUserRequest request, CancellationToken ct);
    Task<BackendAuthTokens> LoginAsync(LoginUserRequest request, CancellationToken ct);
    Task<BackendAuthTokens> RefreshAsync(RefreshTokenRequest request, CancellationToken ct);
    Task LogoutAsync(LogoutUserRequest request, CancellationToken ct);
    Task<BackendMe> GetMeAsync(CancellationToken ct);
}
