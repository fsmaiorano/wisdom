using Wisdom.BuildingBlocks;
using Wisdom.Media.Application.Interfaces;
using Wisdom.Media.Domain.Repositories;
using Wisdom.Media.Domain.ValueObjects;
namespace Wisdom.Media.Application.UseCases;

public class DeleteMediaUseCase(IMediaRepository mediaRepository, IObjectStorageService storageService)
{
    private const string BucketName = "wisdom-media";

    public async Task<Result<bool>> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var getResult = await mediaRepository.GetByIdAsync(MediaId.Of(id), ct);
        if (!getResult.IsSuccess)
            return Result<bool>.Failure(getResult.Error!);

        if (getResult.Data is null)
            return Result<bool>.Failure("Media not found.");

        var media = getResult.Data;

        await storageService.DeleteAsync(BucketName, media.FilePath, ct);

        if (media.ThumbnailPath is not null)
            await storageService.DeleteAsync(BucketName, media.ThumbnailPath, ct);

        return await mediaRepository.DeleteAsync(MediaId.Of(id), ct);
    }
}
