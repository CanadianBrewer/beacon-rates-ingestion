namespace DueDiligenceWorks.Beacon.RateIngestion.Models.Application;

public class BeaconRatesApiConfig
{
    public const string ConfigName = "BeaconRatesApi";
    public required string Url { get; set; }
    public required string ApiKey { get; set; }
    public bool SslHackMode { get; set; }
}