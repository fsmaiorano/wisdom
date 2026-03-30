using Wisdom.BuildingBlocks;
using Wisdom.Course.Application.Dtos;
using Wisdom.Course.Application.Interfaces;

namespace Wisdom.Course.IntegrationTests.Mocks;

public class FakeMediaApiClient : IMediaApiClient
{
    public Task<Result<UploadMediaResponse>> UploadAsync(
        Guid lessonId,
        string mediaType,
        Stream fileContent,
        string fileName,
        string contentType,
        CancellationToken ct = default)
        => Task.FromResult(Result<UploadMediaResponse>.Success(new UploadMediaResponse(Guid.NewGuid())));
}
