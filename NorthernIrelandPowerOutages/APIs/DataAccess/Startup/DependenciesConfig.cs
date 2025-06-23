using DataAccess.Startup;

namespace DataAccess.Startup
{
    public static class DependenciesConfig
    {
        public static void AddDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddOpenApiServices();
            builder.Services.AddCorsServices();
            //builder.Services.AddAllHealthChecks();

            builder.Services.AddTransient<HttpClient>();
        }
    }
}
