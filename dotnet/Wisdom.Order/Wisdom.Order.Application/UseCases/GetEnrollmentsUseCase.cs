using Wisdom.BuildingBlocks;
using Wisdom.Order.Application.Dtos;
using Wisdom.Order.Domain.Repositories;
namespace Wisdom.Order.Application.UseCases;

public class GetEnrollmentsUseCase(IEnrollmentRepository enrollmentRepository)
{
    public async Task<Result<IReadOnlyList<GetEnrollmentResponse>>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await enrollmentRepository.GetByUserIdAsync(userId, cancellationToken);
        if (!result.IsSuccess)
            return Result<IReadOnlyList<GetEnrollmentResponse>>.Failure(result.Error!);

        var responses = result.Data!
            .Select(e => new GetEnrollmentResponse(e.Id.Value, e.UserId, e.CourseId, e.CreatedAt))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<GetEnrollmentResponse>>.Success(responses);
    }
}
