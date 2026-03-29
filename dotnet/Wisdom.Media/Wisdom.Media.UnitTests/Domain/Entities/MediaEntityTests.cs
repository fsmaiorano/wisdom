using Wisdom.Media.Domain.Entities;
using Wisdom.Media.Domain.Enums;
using Wisdom.Tests.Common.Builders;
using Wisdom.Tests.Common.Fakes;
namespace Wisdom.Media.UnitTests.Domain.Entities;

public class MediaEntityTests
{
    private static string CorrelationId => FakeCorrelationIdService.DefaultCorrelationId;

    [Fact]
    public void Create_WithValidData_SetsStatusToUploaded()
    {
        var lessonId = BogusFactory.NewGuid();
        var filePath = BogusFactory.Url();

        var media = MediaEntity.Create(CorrelationId, lessonId, MediaTypeEnum.Video, filePath);

        Assert.Equal(MediaStatusEnum.Uploaded, media.Status);
    }

    [Fact]
    public void Create_SetsCreatedAt()
    {
        var before = DateTime.UtcNow;
        var media = MediaEntity.Create(CorrelationId, BogusFactory.NewGuid(), MediaTypeEnum.Pdf, BogusFactory.Url());
        var after = DateTime.UtcNow;

        Assert.True(media.CreatedAt >= before && media.CreatedAt <= after);
    }

    [Fact]
    public void Create_WithValidData_SetsPropertiesCorrectly()
    {
        var lessonId = BogusFactory.NewGuid();
        var filePath = "media/some-object-name/file.mp4";

        var media = MediaEntity.Create(CorrelationId, lessonId, MediaTypeEnum.Video, filePath);

        Assert.NotNull(media.Id);
        Assert.NotEqual(Guid.Empty, media.Id.Value);
        Assert.Equal(lessonId, media.LessonId);
        Assert.Equal(MediaTypeEnum.Video, media.Type);
        Assert.Equal(filePath, media.FilePath);
        Assert.Null(media.ThumbnailPath);
        Assert.Null(media.Duration);
        Assert.Null(media.LastModifiedAt);
    }

    [Fact]
    public void UpdateStatus_ChangesStatus()
    {
        var media = MediaEntity.Create(CorrelationId, BogusFactory.NewGuid(), MediaTypeEnum.Video, BogusFactory.Url());

        media.UpdateStatus(MediaStatusEnum.Processing);

        Assert.Equal(MediaStatusEnum.Processing, media.Status);
    }

    [Fact]
    public void UpdateStatus_SetsLastModifiedAt()
    {
        var media = MediaEntity.Create(CorrelationId, BogusFactory.NewGuid(), MediaTypeEnum.Video, BogusFactory.Url());
        var before = DateTime.UtcNow;

        media.UpdateStatus(MediaStatusEnum.Failed);

        Assert.NotNull(media.LastModifiedAt);
        Assert.True(media.LastModifiedAt >= before);
    }

    [Fact]
    public void SetReady_SetsStatusReady_UpdatesThumbnailAndDuration()
    {
        var media = MediaEntity.Create(CorrelationId, BogusFactory.NewGuid(), MediaTypeEnum.Video, BogusFactory.Url());
        var thumbnailPath = "media/thumb.jpg";
        var duration = 120;

        media.SetReady(thumbnailPath, duration);

        Assert.Equal(MediaStatusEnum.Ready, media.Status);
        Assert.Equal(thumbnailPath, media.ThumbnailPath);
        Assert.Equal(duration, media.Duration);
        Assert.NotNull(media.LastModifiedAt);
    }

    [Fact]
    public void SetReady_WithNullThumbnailAndDuration_SetsStatusReady()
    {
        var media = MediaEntity.Create(CorrelationId, BogusFactory.NewGuid(), MediaTypeEnum.Image, BogusFactory.Url());

        media.SetReady(null, null);

        Assert.Equal(MediaStatusEnum.Ready, media.Status);
        Assert.Null(media.ThumbnailPath);
        Assert.Null(media.Duration);
    }

    [Fact]
    public void Create_EachCall_ProducesDifferentIds()
    {
        var media1 = MediaEntity.Create(CorrelationId, BogusFactory.NewGuid(), MediaTypeEnum.Video, BogusFactory.Url());
        var media2 = MediaEntity.Create(CorrelationId, BogusFactory.NewGuid(), MediaTypeEnum.Video, BogusFactory.Url());

        Assert.NotEqual(media1.Id.Value, media2.Id.Value);
    }
}
