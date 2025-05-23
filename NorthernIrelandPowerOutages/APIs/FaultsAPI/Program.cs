
using FaultsAPI.Endpoints;
using FaultsAPI.Startup;

namespace FaultsAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

            builder.AddDependencies();

            WebApplication? app = builder.Build();

            app.UseOpenApi();

            app.UseHttpsRedirection();

            app.ApplyCorsConfig();

            app.MapAllHealthChecks();

            app.AddRootEndpoints();
            app.AddErrorEndpoints();
            app.AddFaultEndpoints();

            app.Run();
        }
    }
}
