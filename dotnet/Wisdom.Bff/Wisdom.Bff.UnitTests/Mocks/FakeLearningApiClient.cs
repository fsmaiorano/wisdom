using Wisdom.Bff.Clients;

namespace Wisdom.Bff.UnitTests.Mocks;

public sealed class FakeLearningApiClient : ILearningApiClient
{
    public List<BackendProgress> ProgressResponse { get; set; } = [];

    public int CallCount { get; private set; }
    public IReadOnlyList<Guid>? LastLessonIds { get; private set; }

    public Task<IReadOnlyList<BackendProgress>> GetCourseProgressAsync(
        IReadOnlyList<Guid> lessonIds, CancellationToken ct)
    {
        CallCount++;
        LastLessonIds = lessonIds;
        return Task.FromResult<IReadOnlyList<BackendProgress>>(ProgressResponse);
    }

    public Task<BackendProgress> UpdateProgressAsync(UpdateProgressRequest request, CancellationToken ct) => throw new NotImplementedException();
    public Task<BackendProgress> GetProgressByLessonAsync(Guid lessonId, CancellationToken ct) => throw new NotImplementedException();
}
