using Wisdom.BuildingBlocks;
using Wisdom.Media.Application.Interfaces;
namespace Wisdom.Media.UnitTests.Mocks;

public class FakeMinioService : IMinioService
{
    public bool UploadShouldFail { get; set; }
    public bool GetPresignedUrlShouldFail { get; set; }

    public int UploadCallCount { get; private set; }
    public int GetPresignedUrlCallCount { get; private set; }

    public string? LastBucketName { get; private set; }
    public string? LastObjectName { get; private set; }
    public string? LastContentType { get; private set; }

    public Task<Result<string>> UploadAsync(string bucketName, string objectName, Stream content, string contentType, CancellationToken ct = default)
    {
        UploadCallCount++;
        LastBucketName = bucketName;
        LastObjectName = objectName;
        LastContentType = contentType;

        if (UploadShouldFail)
            return Task.FromResult(Result<string>.Failure("MinIO error: Upload failed."));

        return Task.FromResult(Result<string>.Success(objectName));
    }

    public Task<Result<string>> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 3600, CancellationToken ct = default)
    {
        GetPresignedUrlCallCount++;

        if (GetPresignedUrlShouldFail)
            return Task.FromResult(Result<string>.Failure("MinIO error: GetPresignedUrl failed."));

        return Task.FromResult(Result<string>.Success($"https://fake-minio/{bucketName}/{objectName}?expiry={expirySeconds}"));
    }

    public Task<Result<bool>> DeleteAsync(string bucketName, string objectName, CancellationToken ct = default)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }
}
