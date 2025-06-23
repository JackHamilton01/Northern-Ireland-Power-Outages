using DataAccess.Helpers;
using Domain.Backend;
using Domain.Frontend;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DataAccess.Endpoints
{
    public static class AddressEndpoints
    {
        public static void AddAddressEndpoints(this WebApplication app)
        {
            app.MapGet("/address/{id:int}", LoadAddressesByIdAsync);
            app.MapGet("/address/{userId}/addresses", LoadAddressesAsync);
            app.MapPost("/address/match", FindMatchingAddressAsync);
        }

        private static async Task<IResult> LoadAddressesByIdAsync(HttpContext context, int id)
        {
            ApplicationDbContext? dbContext  = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            var address = await dbContext.Addresses.FindAsync(id);

            if (address is null)
            {
                return Results.BadRequest($"Address with ID {id} not found.");
            }

            return Results.Ok((AddressUI)address);
        }

        private static async Task<IResult> LoadAddressesAsync(HttpContext context, string userId)
        {
            ApplicationDbContext? dbContext  = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            var user = await dbContext.Users
                .Include(u => u.FavoriteAddresses)
                .FirstAsync(u => u.Id == userId);

            if (user is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(user.FavoriteAddresses.Select(a => (AddressUI)a).ToList());
        }

        private static async Task<IResult> FindMatchingAddressAsync(HttpContext context, AddressUI inputAddress)
        {
            ApplicationDbContext? dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            if (!PostCodeHelper.IsValidUkPostcode(inputAddress.PostCode))
            {
                return Results.BadRequest("Invalid UK postcode format.");
            }

            if (dbContext.Addresses is null)
            {
                return Results.NotFound();
            }

            var candidates = await dbContext.Addresses
                .Where(a => a.PostCode == inputAddress.PostCode)
                .ToListAsync();

            if (candidates.Count == 0)
            {
                return Results.Ok(inputAddress);
            }
            return Results.Ok((AddressUI)candidates.FirstOrDefault(a => AddressEquals(inputAddress, a)));
        }

        private static bool AddressEquals(Address firstAddress, Address secondAddress)
        {
            return string.Equals(firstAddress.StreetName?.Trim(), secondAddress.StreetName?.Trim(), StringComparison.OrdinalIgnoreCase)
                && firstAddress.StreetNumber == secondAddress.StreetNumber
                && string.Equals(firstAddress.City?.Trim(), secondAddress.City?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(firstAddress.County?.Trim(), secondAddress.County?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(firstAddress.PostCode?.Trim(), secondAddress.PostCode?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(firstAddress.BuildingDetails?.Trim(), secondAddress.BuildingDetails?.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
