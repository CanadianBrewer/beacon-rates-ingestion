namespace DueDiligenceWorks.Beacon.RateIngestion.Services;

public interface IHealthCheckService
{
    Task<bool> PerformHealthCheck();
}