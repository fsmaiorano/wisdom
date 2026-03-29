using Wisdom.BuildingBlocks;
using Wisdom.Media.Application.Dtos;
using Wisdom.Media.Application.Interfaces;
using Wisdom.Media.Domain.Repositories;
using Wisdom.Media.Domain.ValueObjects;
namespace Wisdom.Media.Application.UseCases;

public class GetMediaUseCase(IMediaRepository mediaRepository, IMinioService minioService)
{
    private const string BucketName = "wisdom-media";

    public async Task<Result<GetMediaResponse>> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var mediaId = MediaId.Of(id);
        var result = await mediaRepository.GetByIdAsync(mediaId, ct);

        if (!result.IsSuccess)
            return Result<GetMediaResponse>.Failure(result.Error!);

        if (result.Data is null)
            return Result<GetMediaResponse>.Failure($"Media with id '{id}' was not found.");

        var media = result.Data;

        var fileUrlResult = await minioService.GetPresignedUrlAsync(BucketName, media.FilePath, ct: ct);
        var fileUrl = fileUrlResult.IsSuccess ? fileUrlResult.Data : null;

        string? thumbnailUrl = null;
        if (media.ThumbnailPath is not null)
        {
            var thumbResult = await minioService.GetPresignedUrlAsync(BucketName, media.ThumbnailPath, ct: ct);
            thumbnailUrl = thumbResult.IsSuccess ? thumbResult.Data : null;
        }

        return Result<GetMediaResponse>.Success(new GetMediaResponse(
            media.Id.Value,
            media.LessonId,
            media.Type.ToString(),
            media.Status.ToString(),
            media.FilePath,
            media.ThumbnailPath,
            media.Duration,
            media.CreatedAt,
            fileUrl,
            thumbnailUrl));
    }
}
