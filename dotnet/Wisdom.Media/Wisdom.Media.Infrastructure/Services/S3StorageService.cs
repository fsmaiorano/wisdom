using Minio;
using Minio.DataModel.Args;
using Wisdom.BuildingBlocks;
using Wisdom.Media.Application.Interfaces;
using Wisdom.Media.Infrastructure.Settings;
namespace Wisdom.Media.Infrastructure.Services;

public class S3StorageService(IMinioClient s3Client) : IObjectStorageService
{
    public async Task<Result<string>> UploadAsync(string bucketName, string objectName, Stream content, string contentType, CancellationToken ct = default)
    {
        try
        {
            var bucketExists = await s3Client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), ct);
            if (!bucketExists)
                await s3Client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), ct);

            Stream uploadStream = content;
            if (!content.CanSeek)
            {
                var ms = new MemoryStream();
                await content.CopyToAsync(ms, ct);
                ms.Position = 0;
                uploadStream = ms;
            }

            await s3Client.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(uploadStream)
                .WithObjectSize(uploadStream.Length)
                .WithContentType(contentType), ct);

            return Result<string>.Success(objectName);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }

    public async Task<Result<string>> GetPresignedUrlAsync(string bucketName, string objectName, int expirySeconds = 3600, CancellationToken ct = default)
    {
        try
        {
            var url = await s3Client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(expirySeconds));
            return Result<string>.Success(url);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string bucketName, string objectName, CancellationToken ct = default)
    {
        try
        {
            await s3Client.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName), ct);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
