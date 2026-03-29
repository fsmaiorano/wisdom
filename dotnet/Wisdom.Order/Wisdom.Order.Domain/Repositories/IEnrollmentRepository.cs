using Wisdom.BuildingBlocks;
using Wisdom.Order.Domain.Entities;
namespace Wisdom.Order.Domain.Repositories;

public interface IEnrollmentRepository
{
    Task<Result<IReadOnlyList<EnrollmentEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Result<bool>> IsEnrolledAsync(Guid userId, Guid courseId, CancellationToken ct = default);
    Task<Result<EnrollmentEntity>> CreateAsync(EnrollmentEntity enrollment, CancellationToken ct = default);
}
