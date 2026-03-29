using Wisdom.BuildingBlocks;
using Wisdom.Order.Application.Dtos;
using Wisdom.Order.Domain.Repositories;
namespace Wisdom.Order.Application.UseCases;

public class CheckEnrollmentUseCase(IEnrollmentRepository enrollmentRepository)
{
    public async Task<Result<CheckEnrollmentResponse>> ExecuteAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
    {
        var result = await enrollmentRepository.IsEnrolledAsync(userId, courseId, cancellationToken);
        if (!result.IsSuccess)
            return Result<CheckEnrollmentResponse>.Failure(result.Error!);

        return Result<CheckEnrollmentResponse>.Success(new CheckEnrollmentResponse(result.Data));
    }
}
