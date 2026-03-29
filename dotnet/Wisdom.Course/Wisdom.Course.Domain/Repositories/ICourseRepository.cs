using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.Domain.Repositories;

public interface ICourseRepository
{
    Task<Result<CourseEntity?>> GetByIdAsync(CourseId id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CourseEntity>>> GetAllPublishedAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CourseEntity>>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default);
    Task<Result<CourseEntity>> CreateAsync(CourseEntity course, CancellationToken cancellationToken = default);
    Task<Result<CourseEntity>> UpdateAsync(CourseEntity course, CancellationToken cancellationToken = default);
}
