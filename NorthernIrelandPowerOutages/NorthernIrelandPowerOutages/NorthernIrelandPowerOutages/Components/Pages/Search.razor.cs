using Domain.Backend;
using Domain.Frontend;
using FaultsAPI.Models;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using NorthernIrelandPowerOutages.Models;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace NorthernIrelandPowerOutages.Components.Pages
{
    public partial class Search
    {
        private FaultModel? fault;
        private string? faultInfo;

        private string? streetNumber = string.Empty;
        private string? streetName = string.Empty;
        private string? postCode = string.Empty;
        private string? errorMessage = string.Empty;

        private List<string> searchResults;
        private bool isSearchResultFavourited = false;

        protected override void OnInitialized()
        {
            FaultPollingService.OnFaultsReceived += FaultPollingService_OnFaultsUpdated;
            fault = FaultPollingService.CurrentFault;
        }

        public async Task SearchByPostCode()
        {
            if (string.IsNullOrWhiteSpace(streetNumber))
            {
                errorMessage = "Street number is required";
                return;
            }
            if (string.IsNullOrWhiteSpace(streetName))
            {
                errorMessage = "Street name is required";
                return;
            }
            if (string.IsNullOrWhiteSpace(postCode))
            {
                errorMessage = "Postcode is required";
                return;
            }

            if (!PostCodeHelper.IsValidUkPostcode(postCode))
            {
                errorMessage = "A valid postcode is required";
                return;
            }

            FaultModel response = await httpClient.GetFromJsonAsync<FaultModel>($"https://localhost:7125/faults?postCode={postCode}");

            if (response.OutageMessage.Length == 0)
            {
                List<string>? addresses = await GeocodeService.GetAddressesFromPostcodeAsync($"{streetName} {streetNumber} {postCode}");
                searchResults = new(addresses);

                return;
            }

            var id = (response.OutageMessage.First()).OutageId;
            var postCodeReceived = (response.OutageMessage.First()).PostCode;
            var estRestoreTime = (response.OutageMessage.First()).EstRestoreTime;

            faultInfo = $"Fault ID: {id}, Post Code: {postCodeReceived}, Estimated Restore Time: {estRestoreTime}";
        }


        private async Task ToggleFavorite(FaultsAPI.Models.OutageMessage outage)
        {
            (double latitude, double longitude) = CoordinateHelpers.ConvertIrishGridToLatLon(outage.Point.Easting, outage.Point.Northing);
            GoogleGeocodeResponse? result = await GeocodeService.GetAddressFromLatLng(latitude, longitude);
            Result? address = result.Results.FirstOrDefault();

            AddressUI addressUI = new()
            {
                StreetNumber = address.AddressComponents[0].LongName, 
                StreetName = address.AddressComponents[1].LongName,
                City = address.AddressComponents[2].LongName,
                County = address.AddressComponents[2].LongName, // City and county from Google maps API are the same
                PostCode = address.AddressComponents[6].LongName,
                Latitude = latitude,
                Longitude = longitude
            };

            await SaveAddressChangesToDatabase(addressUI);
        }

        private async Task ToggleFavoriteFromSearch()
        {
            Location? location = await GeocodeService.GetLatLongFromAddressAsync($"{streetName} {streetNumber} {postCode}");

            GoogleGeocodeResponse? result = await GeocodeService.GetAddressFromLatLng(location.Latitude, location.Longitude);
            Result? address = result.Results.FirstOrDefault();

            AddressUI addressUI = new()
            {
                StreetNumber = address.AddressComponents[0].LongName,
                StreetName = address.AddressComponents[1].LongName,
                City = address.AddressComponents[2].LongName,
                County = address.AddressComponents[2].LongName, // City and county from Google maps API are the same
                PostCode = address.AddressComponents[7].LongName,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };

            await SaveAddressChangesToDatabase(addressUI);
        }

        private async Task SaveAddressChangesToDatabase(AddressUI addressToSave)
        {
            HttpResponseMessage? response = await Http.PostAsJsonAsync("https://localhost:7228/address/match", addressToSave);

            if (response.IsSuccessStatusCode)
            {
                ApplicationUser? authenticatedUser = await GetAuthenticatedUser();

                ApplicationUser user = await DbContext.Users
                    .Include(u => u.FavouriteAddressPreferences)
                    .FirstAsync(u => u.Id == authenticatedUser.Id);

                if (user.FavouriteAddressPreferences is null)
                {
                    user.FavouriteAddressPreferences = new List<FavouriteAddressPreferences>();
                }

                Address addressToAdd;
                bool isExistingAddress = false;
                if (response.Content.Headers.ContentLength == 0)
                {
                    addressToAdd = addressToSave;
                }
                else
                {
                    addressToAdd = await response.Content.ReadFromJsonAsync<AddressUI>();
                    isExistingAddress = true;
                }

                // User has already saved this address
                if (user.FavouriteAddressPreferences.Any(f => f.AddressId == addressToAdd.Id))
                {
                    throw new Exception("This address has already been added to favourites");
                }

                if (!user.FavouriteAddressPreferences.Any(a => a.AddressId == addressToAdd.Id))
                {
                    var geopoint = await GeocodeService.GetLatLongFromAddressAsync(addressToSave.ToString());

                    if (geopoint is null)
                    {
                        throw new Exception("Invalid address provided");
                    }

                    Address backendAddress = addressToAdd;
                    backendAddress.Latitude = geopoint.Latitude;
                    backendAddress.Longitude = geopoint.Longitude;

                    if (!isExistingAddress)
                    {
                        DbContext.Add(backendAddress); 
                    }

                    var newUserAddressPreference = new FavouriteAddressPreferences
                    {
                        ApplicationUserId = user.Id,
                        AddressId = backendAddress.Id,
                        EmailAlertsEnabled = false,
                        SmsAlertsEnabled = false
                    };

                    user.FavouriteAddressPreferences.Add(newUserAddressPreference);
                }

                await DbContext.SaveChangesAsync();
            }

            isSearchResultFavourited = true;
        }

        private async Task<ApplicationUser> GetAuthenticatedUser()
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal claimsPrincipal = authState.User;

            if (claimsPrincipal.Identity?.IsAuthenticated == true)
            {
                string? userId = claimsPrincipal.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userId is not null)
                {
                    return DbContext.Users.First(u => u.Id == userId);
                }
            }

            return null;
        }

        private async void FaultPollingService_OnFaultsUpdated(FaultModel? fault, bool isFirstPoll)
        {
            this.fault = fault;

            await InvokeAsync(StateHasChanged);
        }

    }
}
