using DueDiligenceWorks.Beacon.RateIngestion.Middleware;
using DueDiligenceWorks.Beacon.RateIngestion.Models.Application;
using DueDiligenceWorks.Beacon.RateIngestion.Services;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOptions<FirestoreConfig>()
    .BindConfiguration(FirestoreConfig.ConfigNodeName)
    .ValidateOnStart();
builder.Services.AddSingleton(c => c.GetRequiredService<IOptions<FirestoreConfig>>().Value);

builder.Services.AddScoped<IFirestoreService, FirestoreService>();
builder.Services.AddOptions<BeaconRatesApiConfig>()
    .BindConfiguration(BeaconRatesApiConfig.ConfigName)
    .Validate(config => !string.IsNullOrWhiteSpace(config.ApiKey), "BeaconRatesApi:ApiKey is required")
    .ValidateOnStart();
builder.Services.AddScoped<IBeaconRatesApiService, BeaconRatesApiService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
builder.Services.AddHttpClient<BeaconRatesApiService>();

// the 'UseUrls(...)' line below is required to run in the GCP environment
if (!string.Equals("Development", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), StringComparison.OrdinalIgnoreCase))
{
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}

WebApplication app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
