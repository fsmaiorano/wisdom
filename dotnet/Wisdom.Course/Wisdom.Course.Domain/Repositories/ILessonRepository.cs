using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Domain.Repositories;

public interface ILessonRepository
{
    Task<Result<LessonEntity?>> GetByIdAsync(LessonId id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<LessonEntity>>> GetByCourseIdAsync(CourseId courseId, CancellationToken cancellationToken = default);
    Task<Result<LessonEntity>> CreateAsync(LessonEntity lesson, CancellationToken cancellationToken = default);
    Task<Result<LessonEntity>> UpdateAsync(LessonEntity lesson, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(LessonId id, CancellationToken cancellationToken = default);
}
