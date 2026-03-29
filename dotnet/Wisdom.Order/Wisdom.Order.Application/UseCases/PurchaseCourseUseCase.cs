using Wisdom.BuildingBlocks;
using Wisdom.Order.Application.Dtos;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.Domain.Repositories;
namespace Wisdom.Order.Application.UseCases;

public class PurchaseCourseUseCase(
    IPurchaseRepository purchaseRepository,
    IEnrollmentRepository enrollmentRepository)
{
    public async Task<Result<PurchaseCourseResponse>> ExecuteAsync(Guid userId, PurchaseCourseRequest request, CancellationToken cancellationToken = default)
    {
        var existsResult = await purchaseRepository.ExistsAsync(userId, request.CourseId, cancellationToken);
        if (!existsResult.IsSuccess)
            return Result<PurchaseCourseResponse>.Failure(existsResult.Error!);

        if (existsResult.Data)
            return Result<PurchaseCourseResponse>.Failure("Already purchased");

        var purchase = PurchaseEntity.Create(userId, request.CourseId, request.Price);
        var purchaseResult = await purchaseRepository.CreateAsync(purchase, cancellationToken);
        if (!purchaseResult.IsSuccess)
            return Result<PurchaseCourseResponse>.Failure(purchaseResult.Error!);

        var enrollment = EnrollmentEntity.Create(userId, request.CourseId);
        var enrollmentResult = await enrollmentRepository.CreateAsync(enrollment, cancellationToken);
        if (!enrollmentResult.IsSuccess)
            return Result<PurchaseCourseResponse>.Failure(enrollmentResult.Error!);

        var data = purchaseResult.Data!;
        return Result<PurchaseCourseResponse>.Success(new PurchaseCourseResponse(
            data.Id.Value,
            data.UserId,
            data.CourseId,
            data.Price,
            data.CreatedAt));
    }
}
