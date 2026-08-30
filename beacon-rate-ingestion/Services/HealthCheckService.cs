namespace DueDiligenceWorks.Beacon.RateIngestion.Services;
 
 public class HealthCheckService(ILogger<HealthCheckService> logger, IFirestoreService firestoreService) : IHealthCheckService
 {
     public async Task<bool> PerformHealthCheck()
     {
         logger.LogInformation("Health check started");
         return await firestoreService.CheckFirestoreConnectivityAsync();
     }
 }