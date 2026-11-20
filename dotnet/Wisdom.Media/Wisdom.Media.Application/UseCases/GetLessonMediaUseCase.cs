using Wisdom.BuildingBlocks;
using Wisdom.Media.Application.Dtos;
using Wisdom.Media.Application.Interfaces;
using Wisdom.Media.Domain.Repositories;
namespace Wisdom.Media.Application.UseCases;

public class GetLessonMediaUseCase(IMediaRepository mediaRepository, IObjectStorageService storageService)
{
    private const string BucketName = "wisdom-media";

    public async Task<Result<IReadOnlyList<GetMediaResponse>>> ExecuteAsync(Guid lessonId, CancellationToken ct = default)
    {
        var result = await mediaRepository.GetByLessonIdAsync(lessonId, ct);

        if (!result.IsSuccess)
            return Result<IReadOnlyList<GetMediaResponse>>.Failure(result.Error!);

        var responses = new List<GetMediaResponse>();
        foreach (var media in result.Data!)
        {
            var fileUrlResult = await storageService.GetPresignedUrlAsync(BucketName, media.FilePath, ct: ct);
            var fileUrl = fileUrlResult.IsSuccess ? fileUrlResult.Data : null;

            string? thumbnailUrl = null;
            if (media.ThumbnailPath is not null)
            {
                var thumbResult = await storageService.GetPresignedUrlAsync(BucketName, media.ThumbnailPath, ct: ct);
                thumbnailUrl = thumbResult.IsSuccess ? thumbResult.Data : null;
            }

            responses.Add(new GetMediaResponse(
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

        return Result<IReadOnlyList<GetMediaResponse>>.Success(responses.AsReadOnly());
    }
}
