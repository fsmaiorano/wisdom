using Wisdom.BuildingBlocks;
using Wisdom.Order.Domain.Entities;
using Wisdom.Order.Domain.Repositories;
namespace Wisdom.Order.UnitTests.Mocks;

public class FakeEnrollmentRepository : IEnrollmentRepository
{
    private readonly List<EnrollmentEntity> _store = [];

    public bool CreateShouldFail { get; set; }
    public bool IsEnrolledResult { get; set; }

    public int GetByUserIdCallCount { get; private set; }
    public int IsEnrolledCallCount { get; private set; }
    public int CreateCallCount { get; private set; }

    public EnrollmentEntity? LastCreatedEnrollment { get; private set; }

    public IReadOnlyList<EnrollmentEntity> All => _store.AsReadOnly();

    public FakeEnrollmentRepository Seed(params EnrollmentEntity[] enrollments)
    {
        _store.AddRange(enrollments);
        return this;
    }

    public Task<Result<IReadOnlyList<EnrollmentEntity>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        GetByUserIdCallCount++;
        IReadOnlyList<EnrollmentEntity> result = _store.Where(e => e.UserId == userId).ToList();
        return Task.FromResult(Result<IReadOnlyList<EnrollmentEntity>>.Success(result));
    }

    public Task<Result<bool>> IsEnrolledAsync(Guid userId, Guid courseId, CancellationToken ct = default)
    {
        IsEnrolledCallCount++;
        var result = IsEnrolledResult || _store.Any(e => e.UserId == userId && e.CourseId == courseId);
        return Task.FromResult(Result<bool>.Success(result));
    }

    public Task<Result<EnrollmentEntity>> CreateAsync(EnrollmentEntity enrollment, CancellationToken ct = default)
    {
        CreateCallCount++;
        LastCreatedEnrollment = enrollment;

        if (CreateShouldFail)
            return Task.FromResult(Result<EnrollmentEntity>.Failure("Repository error: Create failed."));

        _store.Add(enrollment);
        return Task.FromResult(Result<EnrollmentEntity>.Success(enrollment));
    }
}
