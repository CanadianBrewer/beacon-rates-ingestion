namespace DueDiligenceWorks.Beacon.RateIngestion.Services;

public interface IBeaconRatesApiService
{
    Task GetAllRates();
    
    // Task GetFixedRates();
    
    Task GetFixedRatesV2();
    
    Task GetIndexedRates();
    
    Task GetRilaRates();
}