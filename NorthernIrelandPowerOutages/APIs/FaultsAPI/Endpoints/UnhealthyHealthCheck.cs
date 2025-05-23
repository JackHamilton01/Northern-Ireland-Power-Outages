using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FaultsAPI.Endpoints
{
    public class UnhealthyHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("This is a test unhealthy service."));
        }
    }
}