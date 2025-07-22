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
                    faultRestoreDate = restoreDate.ToShortDateString();
                }
            }

            var (latitude, longitude) = CoordinateHelpers.ConvertIrishGridToLatLon(Fault.Point.Easting, Fault.Point.Northing);
            GoogleGeocodeResponse? result = await GeocodeService.GetAddressFromLatLng(latitude, longitude);

            Result? address = result.Results.Where(r => !r.Types.Contains("plus_code")).FirstOrDefault();

            faultLocation = $"{address.AddressComponents[0].ShortName} \n {address.AddressComponents[1].ShortName}";
        }

        private async Task AddToFavourites()
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal claimsPrincipal = authState.User;

            if (claimsPrincipal.Identity?.IsAuthenticated == true)
            {
                string? userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is not null)
                {
                    ApplicationUser? user = DbContext.Users.Where(u => u.Id == userId).FirstOrDefault();

                    var (latitude, longitude) = CoordinateHelpers.ConvertIrishGridToLatLon(Fault.Point.Easting, Fault.Point.Northing);
                    GoogleGeocodeResponse? result = await GeocodeService.GetAddressFromLatLng(latitude, longitude);

                    Result? firstAddress = result.Results.FirstOrDefault();

                    Address address = new Address()
                    {
                        Id = 0,
                        City = firstAddress.AddressComponents[2].ShortName,
                        BuildingDetails = "N/A",
                        County = firstAddress.AddressComponents[2].ShortName,
                        Latitude = latitude,
                        Longitude = longitude,
                        PostCode = firstAddress.AddressComponents[8].ShortName,
                        StreetName = firstAddress.AddressComponents[1].ShortName,
                        StreetNumber = firstAddress.AddressComponents[0].ShortName,

                    };
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
                }
            }
        }
    }
}