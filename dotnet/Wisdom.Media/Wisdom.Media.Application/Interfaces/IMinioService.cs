using Wisdom.BuildingBlocks;
namespace Wisdom.Media.Application.Interfaces;

public interface IMinioService
{
    Task<Result<string>> UploadAsync(string bucketName, string objectName, Stream content, string contentType, CancellationToken ct = default);
    Task<Result<string>> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 3600, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(string bucketName, string objectName, CancellationToken ct = default);
}
