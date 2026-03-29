using Wisdom.BuildingBlocks;
using Wisdom.Media.Application.Interfaces;
namespace Wisdom.Course.IntegrationTests.Mocks;

public class FakeMinioService : IMinioService
{
    public Task<Result<string>> UploadAsync(string bucketName, string objectName, Stream content,
        string contentType, CancellationToken ct = default)
        => Task.FromResult(Result<string>.Success(objectName));

    public Task<Result<string>> GetPresignedUrlAsync(string bucketName, string objectName,
        int expirySeconds = 3600, CancellationToken ct = default)
        => Task.FromResult(Result<string>.Success($"http://localhost:9000/{bucketName}/{objectName}"));
}
