using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;

namespace Wisdom.Course.Application.Interfaces;

public interface IMediaApiClient
{
    Task<Result<UploadMediaResponse>> UploadAsync(
        Guid lessonId,
        string mediaType,
        Stream fileContent,
        string fileName,
        string contentType,
        CancellationToken ct = default);
}
