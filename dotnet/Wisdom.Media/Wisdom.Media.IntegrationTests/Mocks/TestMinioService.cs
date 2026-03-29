using Wisdom.BuildingBlocks;
using Wisdom.Media.Application.Interfaces;
namespace Wisdom.Media.IntegrationTests.Mocks;

public class TestMinioService : IMinioService
{
    public bool UploadShouldFail { get; set; }
    public bool GetPresignedUrlShouldFail { get; set; }

    public int UploadCallCount { get; private set; }
    public int GetPresignedUrlCallCount { get; private set; }

    public string? LastBucketName { get; private set; }
    public string? LastObjectName { get; private set; }

    public Task<Result<string>> UploadAsync(string bucketName, string objectName, Stream content, string contentType, CancellationToken ct = default)
    {
        UploadCallCount++;
        LastBucketName = bucketName;
        LastObjectName = objectName;

        if (UploadShouldFail)
            return Task.FromResult(Result<string>.Failure("Test MinIO upload failed."));

        var fakePath = $"test-media/{Guid.NewGuid()}/{objectName.Split('/').Last()}";
        return Task.FromResult(Result<string>.Success(fakePath));
    }

    public Task<Result<string>> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 3600, CancellationToken ct = default)
    {
        GetPresignedUrlCallCount++;

        if (GetPresignedUrlShouldFail)
            return Task.FromResult(Result<string>.Failure("Test MinIO presigned URL failed."));

        return Task.FromResult(Result<string>.Success($"https://test-minio/{bucketName}/{objectName}?expires={expirySeconds}"));
    }
}
