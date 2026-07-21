namespace Wisdom.Bff.Clients;

// Mirrors Wisdom.Course.Application.Dtos
public record BackendCourse(
    Guid CourseId,
    Guid TeacherId,
    string Title,
    string Description,
    decimal Price,
    bool IsPublished,
    DateTime? CreatedAt);

public record BackendLesson(
    Guid LessonId,
    Guid CourseId,
    string Title,
    string Description,
    int Position,
    bool IsPreview,
    DateTime? CreatedAt,
    Guid? MediaId);

// Mirrors Wisdom.Order.Application.Dtos
public record BackendCheckEnrollment(bool IsEnrolled);

public record BackendEnrollment(
    Guid EnrollmentId,
    Guid UserId,
    Guid CourseId,
    DateTime? CreatedAt);

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

// ── Courses ──────────────────────────────────────────────────────────────
public record CreateCourseRequest(string Title, string Description, decimal Price);
public record UpdateCourseRequest(string Title, string Description, decimal Price);
public record PublishCourseRequest(bool Publish);

// ── Lessons ──────────────────────────────────────────────────────────────
public record CreateLessonRequest(string Title, string Description, int Position, bool IsPreview);
public record UpdateLessonRequest(string Title, string Description, int Position, bool IsPreview);

// ── Tags ─────────────────────────────────────────────────────────────────
public record BackendTag(Guid TagId, string Name);
public record CreateTagRequest(string Name);

// ── Media ────────────────────────────────────────────────────────────────
public record BackendMedia(
    Guid MediaId,
    Guid LessonId,
    string Type,
    string Status,
    string FilePath,
    string? ThumbnailPath,
    int? Duration,
    DateTime? CreatedAt,
    string? FileUrl,
    string? ThumbnailUrl);

public record UpdateMediaStatusRequest(string Status);

// ── Orders ───────────────────────────────────────────────────────────────
public record BackendPurchase(Guid PurchaseId, Guid UserId, Guid CourseId, decimal Price, DateTime? CreatedAt);
public record PurchaseCourseRequest(Guid CourseId, decimal Price);

// ── Users / Auth (proxy of Wisdom.User.Api's own /api/auth, /api/users) ───
// Wire-compatible with Wisdom.User.Domain.Enums.UserRoleEnum (Admin=0, Teacher=1, Customer=2).
// Not referenced directly — Bff doesn't take a project reference on Wisdom.User.
public enum BackendUserRole
{
    Admin = 0,
    Teacher = 1,
    Customer = 2,
}

public record RegisterUserRequest(string Email, string Name, string Password, BackendUserRole Role = BackendUserRole.Customer);
public record BackendRegisteredUser(Guid UserId, string KeycloakId, string Email, string Name, BackendUserRole Role);

public record LoginUserRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record LogoutUserRequest(string RefreshToken);
public record BackendAuthTokens(
    string AccessToken, string RefreshToken, int ExpiresIn, int RefreshExpiresIn, string TokenType);

public record BackendMe(Guid? UserId, string? KeycloakId, string Email);
