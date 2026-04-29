using Wisdom.BuildingBlocks;
using Wisdom.Media.Domain.Entities;
using Wisdom.Media.Domain.ValueObjects;
namespace Wisdom.Media.Domain.Repositories;

public interface IMediaRepository
{
    Task<Result<MediaEntity?>> GetByIdAsync(MediaId id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MediaEntity>>> GetByLessonIdAsync(Guid lessonId, CancellationToken ct = default);
    Task<Result<MediaEntity>> CreateAsync(MediaEntity media, CancellationToken ct = default);
    Task<Result<MediaEntity>> UpdateAsync(MediaEntity media, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(MediaId id, CancellationToken ct = default);
}
