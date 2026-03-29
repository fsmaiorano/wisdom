using Wisdom.BuildingBlocks;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.Repositories;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.UnitTests.Mocks;

public class FakeTagRepository : ITagRepository
{
    private readonly List<TagEntity> _store = [];
    private readonly List<CourseTagEntity> _courseTags = [];

    public bool GetAllShouldFail { get; set; }
    public bool GetByIdShouldFail { get; set; }
    public bool CreateShouldFail { get; set; }
    public bool AddTagToCourseShouldFail { get; set; }

    public int CreateCallCount { get; private set; }
    public TagEntity? LastCreatedTag { get; private set; }

    public FakeTagRepository Seed(params TagEntity[] tags)
    {
        _store.AddRange(tags);
        return this;
    }

    public IReadOnlyList<TagEntity> All => _store.AsReadOnly();

    public Task<Result<IReadOnlyList<TagEntity>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (GetAllShouldFail)
            return Task.FromResult(Result<IReadOnlyList<TagEntity>>.Failure("Repository error: GetAll failed."));

        return Task.FromResult(Result<IReadOnlyList<TagEntity>>.Success(_store.AsReadOnly()));
    }

    public Task<Result<TagEntity?>> GetByIdAsync(TagId id, CancellationToken cancellationToken = default)
    {
        if (GetByIdShouldFail)
            return Task.FromResult(Result<TagEntity?>.Failure("Repository error: GetById failed."));

        var tag = _store.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(Result<TagEntity?>.Success(tag));
    }

    public Task<Result<TagEntity>> CreateAsync(TagEntity tag, CancellationToken cancellationToken = default)
    {
        CreateCallCount++;
        LastCreatedTag = tag;

        if (CreateShouldFail)
            return Task.FromResult(Result<TagEntity>.Failure("Repository error: Create failed."));

        _store.Add(tag);
        return Task.FromResult(Result<TagEntity>.Success(tag));
    }

    public Task<Result<bool>> AddTagToCourseAsync(Guid courseId, Guid tagId, CancellationToken cancellationToken = default)
    {
        if (AddTagToCourseShouldFail)
            return Task.FromResult(Result<bool>.Failure("Repository error: AddTagToCourse failed."));

        _courseTags.Add(new CourseTagEntity { CourseId = courseId, TagId = tagId });
        return Task.FromResult(Result<bool>.Success(true));
    }

    public Task<Result<IReadOnlyList<TagEntity>>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var tagIds = _courseTags.Where(ct => ct.CourseId == courseId).Select(ct => ct.TagId).ToList();
        var tags = (IReadOnlyList<TagEntity>)_store.Where(t => tagIds.Contains(t.Id.Value)).ToList();
        return Task.FromResult(Result<IReadOnlyList<TagEntity>>.Success(tags));
    }
}
