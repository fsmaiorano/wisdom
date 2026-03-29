using Wisdom.BuildingBlocks;
using Wisdom.BuildingBlocks.Services.Correlation;
using Wisdom.Media.Application.Dtos;
using Wisdom.Media.Application.Interfaces;
using Wisdom.Media.Domain.Entities;
using Wisdom.Media.Domain.Enums;
using Wisdom.Media.Domain.Repositories;
namespace Wisdom.Media.Application.UseCases;

public class UploadMediaUseCase(IMediaRepository mediaRepository, IMinioService minioService, ICorrelationIdService correlationIdService)
{
    private const string BucketName = "wisdom-media";

    public async Task<Result<GetMediaResponse>> ExecuteAsync(
        Guid lessonId,
        MediaTypeEnum type,
        Stream fileContent,
        string fileName,
        string contentType,
        string? thumbnailPath = null,
        CancellationToken ct = default)
    {
        var existingResult = await mediaRepository.GetByLessonIdAsync(lessonId, ct);
        if (existingResult.IsSuccess && existingResult.Data!.Count > 0)
            return Result<GetMediaResponse>.Failure("A media already exists for this lesson.");

        var objectName = $"media/{Guid.NewGuid()}/{fileName}";

        var uploadResult = await minioService.UploadAsync(BucketName, objectName, fileContent, contentType, ct);
        if (!uploadResult.IsSuccess)
            return Result<GetMediaResponse>.Failure(uploadResult.Error!);

        var media = MediaEntity.Create(
            correlationId: correlationIdService.GetCorrelationId(),
            lessonId: lessonId,
            type: type,
            filePath: objectName,
            thumbnailPath: thumbnailPath
        );

        var createResult = await mediaRepository.CreateAsync(media, ct);
        if (!createResult.IsSuccess)
            return Result<GetMediaResponse>.Failure(createResult.Error!);

        var created = createResult.Data!;

        var fileUrlResult = await minioService.GetPresignedUrlAsync(BucketName, created.FilePath, ct: ct);
        var fileUrl = fileUrlResult.IsSuccess ? fileUrlResult.Data : null;

        return Result<GetMediaResponse>.Success(new GetMediaResponse(
            created.Id.Value,
            created.LessonId,
            created.Type.ToString(),
            created.Status.ToString(),
            created.FilePath,
            created.ThumbnailPath,
            created.Duration,
            created.CreatedAt,
            fileUrl,
            null));
    }
}
