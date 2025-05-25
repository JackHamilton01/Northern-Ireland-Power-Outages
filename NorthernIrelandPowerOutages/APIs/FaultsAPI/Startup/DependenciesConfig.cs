using FaultsAPI.Data;

namespace FaultsAPI.Startup
{
    public static class DependenciesConfig
    {
        public static void AddDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddOpenApiServices();
            builder.Services.AddCorsServices();
            builder.Services.AddAllHealthChecks();

            builder.Services.AddTransient<HttpClient>();
            builder.Services.AddTransient<FaultData>(serviceProvider =>
            {
                return new(serviceProvider.GetRequiredService<HttpClient>()); 
            });
        }
    }
}
