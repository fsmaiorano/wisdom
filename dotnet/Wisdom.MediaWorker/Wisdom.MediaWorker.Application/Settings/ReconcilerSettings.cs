namespace Wisdom.MediaWorker.Application.Settings;

public class ReconcilerSettings
{
    public const string SectionName = "Reconciler";

    /// <summary>How often the reconciler polls for stuck media.</summary>
    public int IntervalSeconds { get; set; } = 60;

    /// <summary>How long a row must stay Uploaded/Failed before being retried.</summary>
    public int StaleAfterSeconds { get; set; } = 300;
}
