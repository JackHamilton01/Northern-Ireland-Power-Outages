using Domain.Backend;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using NorthernIrelandPowerOutages.Models;
using System.Globalization;
using System.Security.Claims;

namespace NorthernIrelandPowerOutages.Components.Overlays
{
    public partial class FaultOverlay
    {
        private readonly string notAvailableMessage = "Not available";
        private readonly string dateTimeFormatString = "h:mm tt, dd MMM yyyy";

        private TimeOnly faultTimeReported;
        private DateOnly faultDateReported;
        private string faultRestoreTime;
        private string faultRestoreDate;
        private string faultLocation;
        private ApplicationUser? user;
        private bool isFavourited = false;

        private void CloseOverlay() => OnClose.InvokeAsync();

        [Parameter]
        public EventCallback OnClose { get; set; }

        [Parameter]
        public OutageMessage Fault { get; set; }

        protected override async Task OnInitializedAsync()
        {
            DateTime dateStarted;

            TimeOnly.TryParse(Fault.StartTime, out faultTimeReported);
            faultTimeReported.ToShortTimeString();

            string inputWithYear = $"{Fault.StartTime} {DateTime.Now.Year}";
            if (DateTime.TryParseExact(
                            inputWithYear,
                            dateTimeFormatString,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateStarted))
            {
                faultDateReported = DateOnly.FromDateTime(dateStarted);
            }

            if (!TimeOnly.TryParse(Fault.EstRestoreTime, out TimeOnly restoreTime))
            {
                faultRestoreTime = notAvailableMessage;
                faultRestoreDate = string.Empty;
            }
            else
            {
                bool hasValidRestoreDate = !DateOnly.TryParse(Fault.EstRestoreFullDateTime, out DateOnly restoreDate);

                if (hasValidRestoreDate)
                {
                    faultRestoreDate = Fault.EstRestoreFullDateTime;
                }
            }

            var (latitude, longitude) = CoordinateHelpers.ConvertIrishGridToLatLon(Fault.Point.Easting, Fault.Point.Northing);
            GoogleGeocodeResponse? result = await GeocodeService.GetAddressFromLatLng(latitude, longitude);
            Result? addressResult = result.Results.Where(r => !r.Types.Contains("plus_code")).FirstOrDefault();

            faultLocation = $"{addressResult.AddressComponents[0].ShortName} \n {addressResult.AddressComponents[1].ShortName}";

            user = await GetAuthenticatedUser();
            Address? address = await GetAddress();

            Address? savedAddress = DbContext.Addresses.FirstOrDefault(a =>
                a.StreetNumber == address.StreetNumber &&
                a.StreetName == address.StreetName);

            if (user is not null && savedAddress is not null)
            {
                FavouriteAddressPreferences? favouriteAddress = DbContext.FavouriteAddressPreferences.FirstOrDefault(f =>
                    f.AddressId == savedAddress.Id &&
                    f.ApplicationUserId == user.Id);

                if (favouriteAddress is not null)
                {
                    isFavourited = true;
                }
            }
        }

        private async Task AddToFavourites()
        {
            ApplicationUser? user = await GetAuthenticatedUser();
            Address? address = await GetAddress();

            DbContext.Addresses.Add(address);
            await DbContext.SaveChangesAsync().ConfigureAwait(false); // Save changes to get ID of address

            DbContext.FavouriteAddressPreferences.Add(new FavouriteAddressPreferences()
            {
                AddressId = address.Id,
                ApplicationUserId = user.Id,
                EmailAlertsEnabled = false,
                SmsAlertsEnabled = false,
            });
            await DbContext.SaveChangesAsync().ConfigureAwait(false);

            isFavourited = true;
        }

        private async Task<Address> GetAddress()
        {
            var (latitude, longitude) = CoordinateHelpers.ConvertIrishGridToLatLon(Fault.Point.Easting, Fault.Point.Northing);
            GoogleGeocodeResponse? result = await GeocodeService.GetAddressFromLatLng(latitude, longitude);

            Result? firstAddress = result.Results.Where(r => r.Types.Contains("premise") || r.Types.Contains("street_address")).FirstOrDefault();

            return new Address()
            {
                Id = 0,
                City = firstAddress.AddressComponents.Where(a => a.Types.Contains("postal_town")).First().ShortName,
                BuildingDetails = "N/A",
                County = firstAddress.AddressComponents.Where(a => a.Types.Contains("postal_town")).First().ShortName,
                Latitude = latitude,
                Longitude = longitude,
                PostCode = firstAddress.AddressComponents.Where(a => a.Types.Contains("postal_code")).First().ShortName,
                StreetName = firstAddress.AddressComponents.Where(a => a.Types.Contains("route")).First().ShortName,
                StreetNumber = firstAddress.AddressComponents.Where(a => a.Types.Contains("street_number")).First().ShortName,

            };
        }

        private async Task<ApplicationUser?> GetAuthenticatedUser()
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal claimsPrincipal = authState.User;

            if (claimsPrincipal.Identity?.IsAuthenticated == true)
            {
                string? userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

                return DbContext.Users.Where(u => u.Id == userId).FirstOrDefault();
            }

            return null;
        }
    }
}