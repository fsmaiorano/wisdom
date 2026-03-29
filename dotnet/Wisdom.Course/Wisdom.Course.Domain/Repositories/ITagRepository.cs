using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Domain.Repositories;

public interface ITagRepository
{
    Task<Result<IReadOnlyList<TagEntity>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<TagEntity?>> GetByIdAsync(TagId id, CancellationToken cancellationToken = default);
    Task<Result<TagEntity>> CreateAsync(TagEntity tag, CancellationToken cancellationToken = default);
    Task<Result<bool>> AddTagToCourseAsync(Guid courseId, Guid tagId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TagEntity>>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
}
