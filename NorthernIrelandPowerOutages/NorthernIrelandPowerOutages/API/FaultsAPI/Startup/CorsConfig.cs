using Scalar.AspNetCore;

namespace FaultsAPI.Startup
{
    /// <summary>
    /// Should only be used in sample APIs, Do not use in production.
    /// </summary>
    public static class CorsConfig
    {
        private const string AllowAllPolicy = "AllowAll";

        public static void AddCorsServices(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(AllowAllPolicy, policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        public static void ApplyCorsConfig(this WebApplication app)
        {
            app.UseCors(AllowAllPolicy);
        }
    }
}
