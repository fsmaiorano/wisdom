using Wisdom.BuildingBlocks;
using Wisdom.Media.Application.Dtos;
using Wisdom.Media.Application.Interfaces;
using Wisdom.Media.Domain.Enums;
using Wisdom.Media.Domain.Repositories;
using Wisdom.Media.Domain.ValueObjects;
namespace Wisdom.Media.Application.UseCases;

public class UpdateMediaStatusUseCase(IMediaRepository mediaRepository, IMinioService minioService)
{
    private const string BucketName = "wisdom-media";

    public async Task<Result<GetMediaResponse>> ExecuteAsync(Guid id, UpdateMediaStatusRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<MediaStatusEnum>(request.Status, ignoreCase: true, out var status))
            return Result<GetMediaResponse>.Failure($"Invalid status value: '{request.Status}'.");

        var mediaId = MediaId.Of(id);
        var getResult = await mediaRepository.GetByIdAsync(mediaId, ct);

        if (!getResult.IsSuccess)
            return Result<GetMediaResponse>.Failure(getResult.Error!);

        if (getResult.Data is null)
            return Result<GetMediaResponse>.Failure($"Media with id '{id}' was not found.");

        var media = getResult.Data;
        media.UpdateStatus(status);

        var updateResult = await mediaRepository.UpdateAsync(media, ct);

        if (!updateResult.IsSuccess)
            return Result<GetMediaResponse>.Failure(updateResult.Error!);

        var updated = updateResult.Data!;

        var fileUrlResult = await minioService.GetPresignedUrlAsync(BucketName, updated.FilePath, ct: ct);
        var fileUrl = fileUrlResult.IsSuccess ? fileUrlResult.Data : null;

        string? thumbnailUrl = null;
        if (updated.ThumbnailPath is not null)
        {
            var thumbResult = await minioService.GetPresignedUrlAsync(BucketName, updated.ThumbnailPath, ct: ct);
            thumbnailUrl = thumbResult.IsSuccess ? thumbResult.Data : null;
        }

        return Result<GetMediaResponse>.Success(new GetMediaResponse(
            updated.Id.Value,
            updated.LessonId,
            updated.Type.ToString(),
            updated.Status.ToString(),
            updated.FilePath,
            updated.ThumbnailPath,
            updated.Duration,
            updated.CreatedAt,
            fileUrl,
            thumbnailUrl));
    }
}
