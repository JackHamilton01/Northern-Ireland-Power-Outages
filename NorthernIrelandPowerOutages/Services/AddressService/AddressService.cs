using Domain.Backend;
using Domain.Frontend;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace AddressService
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext dbContext;

        private readonly Regex ukPostcodeRegex = new Regex(
            @"^([A-Z]{1,2}[0-9][0-9A-Z]?)[ ]?([0-9][A-Z]{2})$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public AddressService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Address> GetAddressByIdAsync(int id)
        {
            return await dbContext.Addresses.FindAsync(id)
                   ?? throw new KeyNotFoundException($"Address with ID {id} not found.");
        }

        public async Task<Address?> GetMatchAsync(AddressUI inputAddress)
        {
            if (!IsValidUkPostcode(inputAddress.PostCode))
            {
                throw new Exception("Invalid UK postcode format.");
            }

            if (dbContext.Addresses is null)
            {
                return null;
            }

            var candidates = await dbContext.Addresses
                .Where(a => a.PostCode == inputAddress.PostCode)
                .ToListAsync();

            return candidates.FirstOrDefault(a => AddressEquals(inputAddress, a));
        }

        public async Task<IEnumerable<Address>> GetFavouriteAddresses(string userId)
        {
            Debug.WriteLine($"Retrieving favorite addresses for user ID: {userId}");

            var user = await dbContext.Users
                .Include(u => u.FavoriteAddresses)
                .FirstAsync(u => u.Id == userId);

            return user.FavoriteAddresses;
        }

        private bool AddressEquals(Address firstAddress, Address secondAddress)
        {
            return string.Equals(firstAddress.StreetName?.Trim(), secondAddress.StreetName?.Trim(), StringComparison.OrdinalIgnoreCase)
                && firstAddress.StreetNumber ==  secondAddress.StreetNumber
                && string.Equals(firstAddress.City?.Trim(), secondAddress.City?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(firstAddress.County?.Trim(), secondAddress.County?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(firstAddress.PostCode?.Trim(), secondAddress.PostCode?.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(firstAddress.BuildingDetails?.Trim(), secondAddress.BuildingDetails?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private bool IsValidUkPostcode(string? postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
            {
                return false;
            }

            postcode = postcode.Trim().ToUpperInvariant();

            return ukPostcodeRegex.IsMatch(postcode);
        }
    }
}
