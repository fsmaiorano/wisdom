using Wisdom.Tests.Common.Builders;
using Wisdom.Learning.Domain.Entities;
namespace Wisdom.Learning.UnitTests.Domain.Entities;

public class ProgressEntityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsProgressEntity()
    {
        var userId = BogusFactory.NewGuid();
        var lessonId = BogusFactory.NewGuid();

        var progress = ProgressEntity.Create(userId, lessonId);

        Assert.NotNull(progress);
        Assert.NotNull(progress.Id);
        Assert.NotEqual(Guid.Empty, progress.Id.Value);
        Assert.Equal(userId, progress.UserId);
        Assert.Equal(lessonId, progress.LessonId);
    }

    [Fact]
    public void Create_SetsWatchedSecondsToZero()
    {
        var progress = ProgressEntity.Create(BogusFactory.NewGuid(), BogusFactory.NewGuid());

        Assert.Equal(0, progress.WatchedSeconds);
    }

    [Fact]
    public void Create_SetsIsCompletedFalse()
    {
        var progress = ProgressEntity.Create(BogusFactory.NewGuid(), BogusFactory.NewGuid());

        Assert.False(progress.IsCompleted);
    }

    [Fact]
    public void Update_ChangesWatchedSecondsAndIsCompleted()
    {
        var progress = ProgressEntity.Create(BogusFactory.NewGuid(), BogusFactory.NewGuid());

        progress.Update(120, true);

        Assert.Equal(120, progress.WatchedSeconds);
        Assert.True(progress.IsCompleted);
    }

    [Fact]
    public void Update_SetsLastModifiedAt()
    {
        var progress = ProgressEntity.Create(BogusFactory.NewGuid(), BogusFactory.NewGuid());
        var before = DateTime.UtcNow;

        progress.Update(60, false);

        Assert.NotNull(progress.LastModifiedAt);
        Assert.True(progress.LastModifiedAt >= before);
    }
}
