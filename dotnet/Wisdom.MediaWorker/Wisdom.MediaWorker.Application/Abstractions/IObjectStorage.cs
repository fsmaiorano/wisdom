namespace Wisdom.MediaWorker.Application.Abstractions;

public interface IObjectStorage
{
    Task DownloadToFileAsync(string objectPath, string localPath, CancellationToken ct = default);

    Task UploadFileAsync(string objectPath, string localPath, string contentType, CancellationToken ct = default);

    Task DeleteAsync(string objectPath, CancellationToken ct = default);
}
