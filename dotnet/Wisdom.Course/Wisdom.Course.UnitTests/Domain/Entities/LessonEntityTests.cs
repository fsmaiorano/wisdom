using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Domain.ValueObjects;
namespace Wisdom.Course.UnitTests.Domain.Entities;

public class LessonEntityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsLessonEntity()
    {
        var courseId = CourseId.Of(Guid.NewGuid());
        var title = "Lesson 1";
        var description = "First lesson";
        var position = 1;
        var isPreview = true;

        var lesson = LessonEntity.Create(courseId, title, description, position, isPreview);

        Assert.NotNull(lesson);
        Assert.NotEqual(Guid.Empty, lesson.Id.Value);
        Assert.Equal(courseId, lesson.CourseId);
        Assert.Equal(title, lesson.Title);
        Assert.Equal(description, lesson.Description);
        Assert.Equal(position, lesson.Position);
        Assert.Equal(isPreview, lesson.IsPreview);
        Assert.NotNull(lesson.CreatedAt);
    }

    [Fact]
    public void Update_ChangesProperties()
    {
        var courseId = CourseId.Of(Guid.NewGuid());
        var lesson = LessonEntity.Create(courseId, "Old Title", "Old Description", 1, false);

        lesson.Update("New Title", "New Description", 2, true);

        Assert.Equal("New Title", lesson.Title);
        Assert.Equal("New Description", lesson.Description);
        Assert.Equal(2, lesson.Position);
        Assert.True(lesson.IsPreview);
        Assert.NotNull(lesson.LastModifiedAt);
    }
}
