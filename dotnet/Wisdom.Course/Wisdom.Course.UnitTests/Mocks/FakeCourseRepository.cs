using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.UnitTests.Mocks;

public class FakeCourseRepository : ICourseRepository
{
    private readonly List<CourseEntity> _store = [];

    public bool GetByIdShouldFail { get; set; }
    public bool GetAllPublishedShouldFail { get; set; }
    public bool GetByTeacherIdShouldFail { get; set; }
    public bool CreateShouldFail { get; set; }
    public bool UpdateShouldFail { get; set; }

    public int GetByIdCallCount { get; private set; }
    public int CreateCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }

    public CourseEntity? LastCreatedCourse { get; private set; }
    public CourseEntity? LastUpdatedCourse { get; private set; }

    public FakeCourseRepository Seed(params CourseEntity[] courses)
    {
        _store.AddRange(courses);
        return this;
    }

    public IReadOnlyList<CourseEntity> All => _store.AsReadOnly();

    public Task<Result<CourseEntity?>> GetByIdAsync(CourseId id, CancellationToken cancellationToken = default)
    {
        GetByIdCallCount++;

        if (GetByIdShouldFail)
            return Task.FromResult(Result<CourseEntity?>.Failure("Repository error: GetById failed."));

        var course = _store.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(Result<CourseEntity?>.Success(course));
    }

    public Task<Result<IReadOnlyList<CourseEntity>>> GetAllPublishedAsync(CancellationToken cancellationToken = default)
    {
        if (GetAllPublishedShouldFail)
            return Task.FromResult(Result<IReadOnlyList<CourseEntity>>.Failure("Repository error: GetAllPublished failed."));

        var courses = (IReadOnlyList<CourseEntity>)_store.Where(c => c.IsPublished).ToList();
        return Task.FromResult(Result<IReadOnlyList<CourseEntity>>.Success(courses));
    }

    public Task<Result<IReadOnlyList<CourseEntity>>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        if (GetByTeacherIdShouldFail)
            return Task.FromResult(Result<IReadOnlyList<CourseEntity>>.Failure("Repository error: GetByTeacherId failed."));

        var courses = (IReadOnlyList<CourseEntity>)_store.Where(c => c.TeacherId == teacherId).ToList();
        return Task.FromResult(Result<IReadOnlyList<CourseEntity>>.Success(courses));
    }

    public Task<Result<CourseEntity>> CreateAsync(CourseEntity course, CancellationToken cancellationToken = default)
    {
        CreateCallCount++;
        LastCreatedCourse = course;

        if (CreateShouldFail)
            return Task.FromResult(Result<CourseEntity>.Failure("Repository error: Create failed."));

        _store.Add(course);
        return Task.FromResult(Result<CourseEntity>.Success(course));
    }

    public Task<Result<CourseEntity>> UpdateAsync(CourseEntity course, CancellationToken cancellationToken = default)
    {
        UpdateCallCount++;
        LastUpdatedCourse = course;

        if (UpdateShouldFail)
            return Task.FromResult(Result<CourseEntity>.Failure("Repository error: Update failed."));

        return Task.FromResult(Result<CourseEntity>.Success(course));
    }
}
