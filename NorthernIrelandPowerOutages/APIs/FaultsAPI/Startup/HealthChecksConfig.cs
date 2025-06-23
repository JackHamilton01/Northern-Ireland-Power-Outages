using FaultsAPI.Endpoints;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace FaultsAPI.Startup
{
    public static class HealthChecksConfig
    {
        public static void AddAllHealthChecks(this IServiceCollection services)
        {
            services.AddHealthChecks()
                //.AddCheck<HealthyHealthCheck>("Healthy", tags: new[] { "healthy" })
                //.AddCheck<DegradedHealthCheck>("Degraded", tags: new[] { "degraded" })
                .AddCheck<UnhealthyHealthCheck>("Unhealthy", tags: new[] { "unhealthy" })
                .AddCheck<RandomHealthCheck>("Random", tags: new[] { "random" });
        }

        public static void MapAllHealthChecks(this WebApplication app)
        {
            app.MapHealthChecks("/health");

            app.MapHealthChecks("/health/healthy", new HealthCheckOptions
            {
                Predicate = x => x.Tags.Contains("healthy")
            });
            app.MapHealthChecks("/health/degraded", new HealthCheckOptions
            {
                Predicate = x => x.Tags.Contains("degraded")
            });
            app.MapHealthChecks("/health/unhealthy", new HealthCheckOptions
            {
                Predicate = x => x.Tags.Contains("unhealthy")
            });
            app.MapHealthChecks("/health/random", new HealthCheckOptions
            {
                Predicate = x => x.Tags.Contains("random")
            });
            app.MapHealthChecks("/health/ui", new HealthCheckOptions()
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

            app.MapHealthChecks("/health/ui/healthy", new HealthCheckOptions
            {
                Predicate = x => x.Tags.Contains("healthy"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });
            app.MapHealthChecks("/health/ui/degraded", new HealthCheckOptions
            {
                Predicate = x => x.Tags.Contains("degraded"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });
            app.MapHealthChecks("/health/ui/unhealthy", new HealthCheckOptions
            {
                Predicate = x => x.Tags.Contains("unhealthy"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });
            app.MapHealthChecks("/health/ui/random", new HealthCheckOptions
            {
                Predicate = x => x.Tags.Contains("random"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });
        }
    }
}