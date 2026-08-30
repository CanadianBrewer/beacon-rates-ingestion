namespace DueDiligenceWorks.Beacon.RateIngestion.Models.Application;

public class FirestoreConfig
{
    public const string ConfigNodeName = "FirestoreSettings";
    public required string ProjectName { get; init; }
    public bool UseEmulator { get; init; }
}