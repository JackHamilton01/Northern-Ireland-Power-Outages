using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FaultsAPI.HealthChecks
{
    public class HealthyHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("This is a test healthy service."));
        }
    }
}
