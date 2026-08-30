namespace DueDiligenceWorks.Beacon.RateIngestion.Services;

public interface IBeaconRatesApiService
{
    Task GetAllRates();
    
    Task GetFixedRates();
    
    Task GetIndexedRates();
    
    Task GetRilaRates();
}