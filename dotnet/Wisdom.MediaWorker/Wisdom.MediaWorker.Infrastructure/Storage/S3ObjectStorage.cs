using Minio;
using Minio.DataModel.Args;
using Wisdom.MediaWorker.Application.Abstractions;
using Wisdom.MediaWorker.Infrastructure.Settings;
namespace Wisdom.MediaWorker.Infrastructure.Storage;

public class S3ObjectStorage(IMinioClient client, StorageSettings settings) : IObjectStorage
{
    public async Task DownloadToFileAsync(string objectPath, string localPath, CancellationToken ct = default)
        => await client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(settings.BucketName)
            .WithObject(objectPath)
            .WithFile(localPath), ct);

    public async Task UploadFileAsync(string objectPath, string localPath, string contentType,
        CancellationToken ct = default)
        => await client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(settings.BucketName)
            .WithObject(objectPath)
            .WithFileName(localPath)
            .WithContentType(contentType), ct);

    public async Task DeleteAsync(string objectPath, CancellationToken ct = default)
        => await client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(settings.BucketName)
            .WithObject(objectPath), ct);
}
