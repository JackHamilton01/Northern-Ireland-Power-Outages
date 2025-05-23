using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FaultsAPI.Endpoints
{
    public class RandomHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            int randomResult = Random.Shared.Next(1, 4);

            return randomResult switch
            {
                1 => Task.FromResult(HealthCheckResult.Healthy("Random Health Check is healthy.")),
                2 => Task.FromResult(HealthCheckResult.Degraded("Random Health Check is degraded.")),
                3 => Task.FromResult(HealthCheckResult.Unhealthy("Random Health Check is unhealthy.")),
                _ => Task.FromResult(HealthCheckResult.Healthy("Random Health random service."))
            };
        }
    }
}
