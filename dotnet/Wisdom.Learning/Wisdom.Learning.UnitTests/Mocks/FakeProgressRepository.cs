using Wisdom.BuildingBlocks;
using Wisdom.Learning.Domain.Entities;
using Wisdom.Learning.Domain.Repositories;
using Wisdom.Learning.Domain.ValueObjects;
namespace Wisdom.Learning.UnitTests.Mocks;

public class FakeProgressRepository : IProgressRepository
{
    private readonly List<ProgressEntity> _store = [];

    public bool CreateShouldFail { get; set; }
    public bool UpdateShouldFail { get; set; }

    public int GetByIdCallCount { get; private set; }
    public int GetByUserAndLessonCallCount { get; private set; }
    public int GetByUserAndLessonIdsCallCount { get; private set; }
    public int CreateCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }

    public ProgressEntity? LastCreatedProgress { get; private set; }
    public ProgressEntity? LastUpdatedProgress { get; private set; }

    public ProgressEntity? GetByUserAndLessonResult { get; set; } = null;

    public FakeProgressRepository Seed(params ProgressEntity[] entities)
    {
        _store.AddRange(entities);
        return this;
    }

    public IReadOnlyList<ProgressEntity> All => _store.AsReadOnly();

    public Task<Result<ProgressEntity?>> GetByIdAsync(ProgressId id, CancellationToken ct = default)
    {
        GetByIdCallCount++;
        var progress = _store.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(Result<ProgressEntity?>.Success(progress));
    }

    public Task<Result<ProgressEntity?>> GetByUserAndLessonAsync(Guid userId, Guid lessonId, CancellationToken ct = default)
    {
        GetByUserAndLessonCallCount++;

        if (GetByUserAndLessonResult is not null)
            return Task.FromResult(Result<ProgressEntity?>.Success(GetByUserAndLessonResult));

        var progress = _store.FirstOrDefault(p => p.UserId == userId && p.LessonId == lessonId);
        return Task.FromResult(Result<ProgressEntity?>.Success(progress));
    }

    public Task<Result<IReadOnlyList<ProgressEntity>>> GetByUserAndLessonIdsAsync(Guid userId, IEnumerable<Guid> lessonIds, CancellationToken ct = default)
    {
        GetByUserAndLessonIdsCallCount++;
        var ids = lessonIds.ToList();
        var result = _store
            .Where(p => p.UserId == userId && ids.Contains(p.LessonId))
            .ToList();
        return Task.FromResult(Result<IReadOnlyList<ProgressEntity>>.Success(result.AsReadOnly()));
    }

    public Task<Result<ProgressEntity>> CreateAsync(ProgressEntity progress, CancellationToken ct = default)
    {
        CreateCallCount++;
        LastCreatedProgress = progress;

        if (CreateShouldFail)
            return Task.FromResult(Result<ProgressEntity>.Failure("Repository error: Create failed."));

        _store.Add(progress);
        return Task.FromResult(Result<ProgressEntity>.Success(progress));
    }

    public Task<Result<ProgressEntity>> UpdateAsync(ProgressEntity progress, CancellationToken ct = default)
    {
        UpdateCallCount++;
        LastUpdatedProgress = progress;

        if (UpdateShouldFail)
            return Task.FromResult(Result<ProgressEntity>.Failure("Repository error: Update failed."));

        var index = _store.FindIndex(p => p.Id == progress.Id);
        if (index >= 0)
            _store[index] = progress;

        return Task.FromResult(Result<ProgressEntity>.Success(progress));
    }
}
