using Wisdom.MediaWorker.Application.Abstractions;
namespace Wisdom.MediaWorker.IntegrationTests.Mocks;

public class FakeObjectStorage : IObjectStorage
{
    public List<string> Downloaded { get; } = [];
    public Dictionary<string, string> Uploaded { get; } = [];
    public List<string> Deleted { get; } = [];
    public bool FailDownload { get; set; }
    public bool FailDelete { get; set; }

    public Task DownloadToFileAsync(string objectPath, string localPath, CancellationToken ct = default)
    {
        if (FailDownload)
            throw new InvalidOperationException("object not found");

        Downloaded.Add(objectPath);
        File.WriteAllText(localPath, "raw");
        return Task.CompletedTask;
    }

    public Task UploadFileAsync(string objectPath, string localPath, string contentType, CancellationToken ct = default)
    {
        Uploaded[objectPath] = contentType;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string objectPath, CancellationToken ct = default)
    {
        if (FailDelete)
            throw new InvalidOperationException("delete failed");

        Deleted.Add(objectPath);
        return Task.CompletedTask;
    }
}
