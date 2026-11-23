namespace Wisdom.MediaWorker.Infrastructure.Settings;

public class StorageSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSSL { get; set; }
    public string BucketName { get; set; } = "wisdom-media";
}
