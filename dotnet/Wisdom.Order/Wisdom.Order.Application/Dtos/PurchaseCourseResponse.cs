namespace Wisdom.Order.Application.Dtos;

public record PurchaseCourseResponse(Guid PurchaseId, Guid UserId, Guid CourseId, decimal Price, DateTime? CreatedAt);
