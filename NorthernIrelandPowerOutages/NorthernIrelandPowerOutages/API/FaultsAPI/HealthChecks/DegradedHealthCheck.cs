using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FaultsAPI.HealthChecks
{
    public class DegradedHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Degraded("This is a test degraded service."));
        }
    }
}
