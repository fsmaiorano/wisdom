using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.UnitTests.Mocks;

public class FakeLessonRepository : ILessonRepository
{
    private readonly List<LessonEntity> _store = [];

    public bool GetByIdShouldFail { get; set; }
    public bool GetByCourseIdShouldFail { get; set; }
    public bool CreateShouldFail { get; set; }
    public bool UpdateShouldFail { get; set; }
    public bool DeleteShouldFail { get; set; }

    public int CreateCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }
    public int DeleteCallCount { get; private set; }

    public LessonEntity? LastCreatedLesson { get; private set; }
    public LessonEntity? LastUpdatedLesson { get; private set; }

    public FakeLessonRepository Seed(params LessonEntity[] lessons)
    {
        _store.AddRange(lessons);
        return this;
    }

    public IReadOnlyList<LessonEntity> All => _store.AsReadOnly();

    public Task<Result<LessonEntity?>> GetByIdAsync(LessonId id, CancellationToken cancellationToken = default)
    {
        if (GetByIdShouldFail)
            return Task.FromResult(Result<LessonEntity?>.Failure("Repository error: GetById failed."));

        var lesson = _store.FirstOrDefault(l => l.Id == id);
        return Task.FromResult(Result<LessonEntity?>.Success(lesson));
    }

    public Task<Result<IReadOnlyList<LessonEntity>>> GetByCourseIdAsync(CourseId courseId, CancellationToken cancellationToken = default)
    {
        if (GetByCourseIdShouldFail)
            return Task.FromResult(Result<IReadOnlyList<LessonEntity>>.Failure("Repository error: GetByCourseId failed."));

        var lessons = (IReadOnlyList<LessonEntity>)_store.Where(l => l.CourseId == courseId).ToList();
        return Task.FromResult(Result<IReadOnlyList<LessonEntity>>.Success(lessons));
    }

    public Task<Result<LessonEntity>> CreateAsync(LessonEntity lesson, CancellationToken cancellationToken = default)
    {
        CreateCallCount++;
        LastCreatedLesson = lesson;

        if (CreateShouldFail)
            return Task.FromResult(Result<LessonEntity>.Failure("Repository error: Create failed."));

        _store.Add(lesson);
        return Task.FromResult(Result<LessonEntity>.Success(lesson));
    }

    public Task<Result<LessonEntity>> UpdateAsync(LessonEntity lesson, CancellationToken cancellationToken = default)
    {
        UpdateCallCount++;
        LastUpdatedLesson = lesson;

        if (UpdateShouldFail)
            return Task.FromResult(Result<LessonEntity>.Failure("Repository error: Update failed."));

        return Task.FromResult(Result<LessonEntity>.Success(lesson));
    }

    public Task<Result<bool>> DeleteAsync(LessonId id, CancellationToken cancellationToken = default)
    {
        DeleteCallCount++;

        if (DeleteShouldFail)
            return Task.FromResult(Result<bool>.Failure("Repository error: Delete failed."));

        var lesson = _store.FirstOrDefault(l => l.Id == id);
        if (lesson is not null) _store.Remove(lesson);
        return Task.FromResult(Result<bool>.Success(true));
    }
}
