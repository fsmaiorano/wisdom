namespace Wisdom.Order.Application.Dtos;

public record GetEnrollmentResponse(Guid EnrollmentId, Guid UserId, Guid CourseId, DateTime? CreatedAt);
