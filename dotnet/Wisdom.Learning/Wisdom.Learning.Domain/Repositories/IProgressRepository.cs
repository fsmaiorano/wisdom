using Wisdom.BuildingBlocks;
using Wisdom.Learning.Domain.Entities;
using Wisdom.Learning.Domain.ValueObjects;
namespace Wisdom.Learning.Domain.Repositories;

public interface IProgressRepository
{
    Task<Result<ProgressEntity?>> GetByIdAsync(ProgressId id, CancellationToken ct = default);
    Task<Result<ProgressEntity?>> GetByUserAndLessonAsync(Guid userId, Guid lessonId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProgressEntity>>> GetByUserAndLessonIdsAsync(Guid userId, IEnumerable<Guid> lessonIds, CancellationToken ct = default);
    Task<Result<ProgressEntity>> CreateAsync(ProgressEntity progress, CancellationToken ct = default);
    Task<Result<ProgressEntity>> UpdateAsync(ProgressEntity progress, CancellationToken ct = default);
}
