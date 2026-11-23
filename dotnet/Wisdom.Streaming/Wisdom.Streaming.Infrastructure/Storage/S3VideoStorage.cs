using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Wisdom.Streaming.Application.Abstractions;
namespace Wisdom.Streaming.Infrastructure.Storage;

public class S3VideoStorage(IMinioClient client, StorageSettings settings) : IVideoStorage
{
    public async Task<long?> GetSizeAsync(string objectPath, CancellationToken ct = default)
    {
        try
        {
            var stat = await client.StatObjectAsync(new StatObjectArgs()
                .WithBucket(settings.BucketName)
                .WithObject(objectPath), ct);
            return stat.Size;
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
    }

    public async Task CopyRangeAsync(string objectPath, long offset, long length, Stream destination,
        CancellationToken ct = default)
        => await client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(settings.BucketName)
            .WithObject(objectPath)
            .WithOffsetAndLength(offset, length)
            .WithCallbackStream((stream, token) => stream.CopyToAsync(destination, token)), ct);
}
