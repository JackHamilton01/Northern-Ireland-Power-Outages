using Domain.Frontend;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Endpoints
{
    public static class ServiceEndpoints
    {
        public static void AddServiceEndpoints(this WebApplication app)
        {
            app.MapGet("/services/{id:int}", LoadServiceById);
            app.MapGet("/services", async (ApplicationDbContext dbContext) => await LoadServicesAsync(dbContext));
        }

        private static async Task<IResult> LoadServicesAsync(ApplicationDbContext dbContext)
        {
            var services = await dbContext.Services
                .ToListAsync();
            return Results.Ok(services);
        }

        private static async Task<IResult> LoadServiceById(HttpContext context, int id)
        {
            ApplicationDbContext? dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            var service = dbContext.Services.FirstOrDefault(h => h.Id == id);

            return Results.Ok((ServiceUI)service);
        }
    }
}
