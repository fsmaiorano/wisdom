using Wisdom.BuildingBlocks;
using Wisdom.Media.Domain.Entities;
using Wisdom.Media.Domain.Repositories;
using Wisdom.Media.Domain.ValueObjects;
namespace Wisdom.Media.UnitTests.Mocks;

public class FakeMediaRepository : IMediaRepository
{
    private readonly List<MediaEntity> _store = [];

    public bool GetByIdShouldFail { get; set; }
    public bool GetByLessonIdShouldFail { get; set; }
    public bool CreateShouldFail { get; set; }
    public bool UpdateShouldFail { get; set; }

    public int GetByIdCallCount { get; private set; }
    public int GetByLessonIdCallCount { get; private set; }
    public int CreateCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }

    public MediaEntity? LastCreatedMedia { get; private set; }
    public MediaEntity? LastUpdatedMedia { get; private set; }

    public FakeMediaRepository Seed(params MediaEntity[] entities)
    {
        _store.AddRange(entities);
        return this;
    }

    public IReadOnlyList<MediaEntity> All => _store.AsReadOnly();

    public Task<Result<MediaEntity?>> GetByIdAsync(MediaId id, CancellationToken ct = default)
    {
        GetByIdCallCount++;

        if (GetByIdShouldFail)
            return Task.FromResult(Result<MediaEntity?>.Failure("Repository error: GetById failed."));

        var media = _store.FirstOrDefault(m => m.Id == id);
        return Task.FromResult(Result<MediaEntity?>.Success(media));
    }

    public Task<Result<IReadOnlyList<MediaEntity>>> GetByLessonIdAsync(Guid lessonId, CancellationToken ct = default)
    {
        GetByLessonIdCallCount++;

        if (GetByLessonIdShouldFail)
            return Task.FromResult(Result<IReadOnlyList<MediaEntity>>.Failure("Repository error: GetByLessonId failed."));

        IReadOnlyList<MediaEntity> result = _store.Where(m => m.LessonId == lessonId).ToList().AsReadOnly();
        return Task.FromResult(Result<IReadOnlyList<MediaEntity>>.Success(result));
    }

    public Task<Result<MediaEntity>> CreateAsync(MediaEntity media, CancellationToken ct = default)
    {
        CreateCallCount++;
        LastCreatedMedia = media;

        if (CreateShouldFail)
            return Task.FromResult(Result<MediaEntity>.Failure("Repository error: Create failed."));

        _store.Add(media);
        return Task.FromResult(Result<MediaEntity>.Success(media));
    }

    public Task<Result<MediaEntity>> UpdateAsync(MediaEntity media, CancellationToken ct = default)
    {
        UpdateCallCount++;
        LastUpdatedMedia = media;

        if (UpdateShouldFail)
            return Task.FromResult(Result<MediaEntity>.Failure("Repository error: Update failed."));

        var index = _store.FindIndex(m => m.Id == media.Id);
        if (index >= 0)
            _store[index] = media;

        return Task.FromResult(Result<MediaEntity>.Success(media));
    }

    public Task<Result<bool>> DeleteAsync(MediaId id, CancellationToken ct = default)
    {
        var removed = _store.RemoveAll(m => m.Id == id) > 0;
        return Task.FromResult(removed
            ? Result<bool>.Success(true)
            : Result<bool>.Failure("Media not found."));
    }
}
