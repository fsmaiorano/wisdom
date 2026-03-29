using Wisdom.Course.Domain.Entities;
namespace Wisdom.Course.UnitTests.Domain.Entities;

public class CourseEntityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsCourseEntity()
    {
        var teacherId = Guid.NewGuid();
        var title = "Introduction to .NET";
        var description = "A comprehensive .NET course";
        var price = 99.99m;

        var course = CourseEntity.Create(teacherId, title, description, price);

        Assert.NotNull(course);
        Assert.NotEqual(Guid.Empty, course.Id.Value);
        Assert.Equal(teacherId, course.TeacherId);
        Assert.Equal(title, course.Title);
        Assert.Equal(description, course.Description);
        Assert.Equal(price, course.Price);
    }

    [Fact]
    public void Create_SetsIsPublishedFalse()
    {
        var course = CourseEntity.Create(Guid.NewGuid(), "Title", "Description", 10m);

        Assert.False(course.IsPublished);
    }

    [Fact]
    public void Create_SetsCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var course = CourseEntity.Create(Guid.NewGuid(), "Title", "Description", 10m);

        Assert.NotNull(course.CreatedAt);
        Assert.True(course.CreatedAt >= before);
        Assert.True(course.CreatedAt <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Update_ChangesProperties_SetsLastModifiedAt()
    {
        var course = CourseEntity.Create(Guid.NewGuid(), "Old Title", "Old Description", 10m);

        course.Update("New Title", "New Description", 50m);

        Assert.Equal("New Title", course.Title);
        Assert.Equal("New Description", course.Description);
        Assert.Equal(50m, course.Price);
        Assert.NotNull(course.LastModifiedAt);
    }

    [Fact]
    public void Publish_SetsIsPublishedTrue()
    {
        var course = CourseEntity.Create(Guid.NewGuid(), "Title", "Description", 10m);

        course.Publish();

        Assert.True(course.IsPublished);
        Assert.NotNull(course.LastModifiedAt);
    }

    [Fact]
    public void Unpublish_SetsIsPublishedFalse()
    {
        var course = CourseEntity.Create(Guid.NewGuid(), "Title", "Description", 10m);
        course.Publish();

        course.Unpublish();

        Assert.False(course.IsPublished);
        Assert.NotNull(course.LastModifiedAt);
    }
}
