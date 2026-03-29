namespace Wisdom.Simulator.Models;

public record PurchaseCourseRequest(Guid CourseId, decimal Price);
public record PurchaseCourseResponse(Guid PurchaseId, Guid UserId, Guid CourseId, decimal Price, DateTime? CreatedAt);
