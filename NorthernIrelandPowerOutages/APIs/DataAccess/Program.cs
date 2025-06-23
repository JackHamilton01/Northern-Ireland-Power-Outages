using DataAccess.Endpoints;
using DataAccess.Startup;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

            builder.Services.AddOpenApiServices();
            builder.AddDependencies();

            WebApplication? app = builder.Build();

            app.UseOpenApi();

            app.UseHttpsRedirection();

            app.ApplyCorsConfig();

            //app.MapAllHealthChecks();

            app.AddRootEndpoints();
            app.AddErrorEndpoints();
            app.AddAddressEndpoints();

            app.Run();
        }
    }
}
