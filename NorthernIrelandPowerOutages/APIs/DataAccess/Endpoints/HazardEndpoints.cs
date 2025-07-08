using Domain.Frontend;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Endpoints
{
    public static class HazardEndpoints
    {
        public static void AddhazardEndpoints(this WebApplication app)
        {
            app.MapGet("/hazards/{id:int}", LoadHazardByIdAsync);
            app.MapGet("/hazards", async (ApplicationDbContext dbContext) => await LoadHazardsAsync(dbContext));
            app.MapPost("/hazards/location", LoadHazardByLatitudeAndLongitude);
        }

        private static async Task<IResult> LoadHazardsAsync(ApplicationDbContext dbContext)
        {
            var hazards = await dbContext.Hazards
                .Include(h => h.FileNames)
                .ToListAsync();
            return Results.Ok(hazards);
        }

        private static async Task<IResult> LoadHazardByIdAsync(HttpContext context, int id)
        {
            ApplicationDbContext? dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            var hazard = dbContext.Hazards.FirstOrDefault(h => h.Id == id);

            return Results.Ok((HazardUI)hazard);
        }

        private static async Task<IResult> LoadHazardByLatitudeAndLongitude(
            HttpContext context,
            double[] geoPoint)
        {
            ApplicationDbContext? dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            var hazard = dbContext.Hazards
                .Include(h => h.FileNames)
                .FirstOrDefault(h => h.Latitude == geoPoint[0] && h.Longitude == geoPoint[1]);

            if (hazard == null)
            {
                return Results.NotFound();
            }

            return Results.Ok((HazardUI)hazard);
        }
    }
}
