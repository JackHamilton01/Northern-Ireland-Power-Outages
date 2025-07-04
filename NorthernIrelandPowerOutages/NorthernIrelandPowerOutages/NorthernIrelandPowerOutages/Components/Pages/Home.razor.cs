﻿using Domain.Backend;
using Domain.Frontend;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using NorthernIrelandPowerOutages.Counties;
using NorthernIrelandPowerOutages.Enums;
using NorthernIrelandPowerOutages.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace NorthernIrelandPowerOutages.Components.Pages
{
    public partial class Home : ComponentBase
    {
        private IJSObjectReference? module;
        private DotNetObjectReference<Home> dotNetObjectReference;
        private int zoomLevel = 8;

        private bool showModal = false;
        private bool isCalculatingCountyOutages = true;

        private bool isCalculatingCountyOutagesStarted = false;
        private bool mapInitialised;
        private bool mapMarkersPlaced = false;

        private GoogleMapPin? googleMapPin;
        private CountyMatcher countyMatcher;
        private GoogleMapPin? homePin;

        private bool IsHomePage =>
           string.IsNullOrEmpty(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));

        private Dictionary<string, int> countyOutages =
        NorthernIrelandCounties.Counties.ToDictionary(county => county,
        _ => 0,
        StringComparer.OrdinalIgnoreCase);

        List<GoogleMapPin> markers = new();

        protected async override Task OnInitializedAsync()
        {
            countyMatcher = await CountyMatcher.Create();
            dotNetObjectReference = DotNetObjectReference.Create(this);

            mapInitialised = true;
        }

        private async Task DisplayFaults(FaultModel? faults)
        {
            if (faults is null)
            {
                return;
            }

            foreach (var outageMessage in faults.OutageMessage)
            {
                if (!string.IsNullOrWhiteSpace(outageMessage.Point.Coordinates))
                {
                    (double latitude, double longitude) = CoordinateHelpers.ConvertIrishGridToLatLon(outageMessage.Point.Easting, outageMessage.Point.Northing);

                    string icon = outageMessage.OutageType == "Planned" ? GoogleMapPinIconConstants.Planned : GoogleMapPinIconConstants.Default;

                    markers.Add(new GoogleMapPin()
                    {
                        Name = outageMessage.PostCode,
                        StatusMessage = outageMessage.StatusMessage,
                        Latitude = latitude,
                        Longitude = longitude,
                        IsFault = true,
                        Icon = icon,
                    });
                }
            }

            if (homePin is not null)
            {
                markers.Add(homePin);
            }

            Debug.WriteLine("Getting favourite addresses");
            var favouriteAddresses = await GetFavouriteAddresses();
            if (favouriteAddresses is not null)
            {
                foreach (var address in favouriteAddresses)
                {
                    markers.Add(new GoogleMapPin()
                    {
                        Name = address.PostCode,
                        StatusMessage = string.Empty,
                        Latitude = address.Latitude,
                        Longitude = address.Longitude,
                        IsFault = false,
                        Icon = GoogleMapPinIconConstants.Favourite,
                    });
                }
            }
        }

        private async void FaultPollingService_OnFaultsUpdated(FaultModel? faultData, bool isFirstPoll)
        {
            await DisplayFaults(faultData);

            if (IsHomePage)
            {
                await module.InvokeVoidAsync("updateMarkers", markers, dotNetObjectReference);
                await CalculateCountyOutages();
            }

            if (isFirstPoll && homePin is not null)
            {
                //await module.InvokeVoidAsync("MoveToLocation", homePin.Latitude, homePin.Longitude);
            }

            await InvokeAsync(StateHasChanged);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Home.razor.js");

                Debug.WriteLine("---");
                Debug.WriteLine("Subscribed");
                FaultPollingService.OnFaultsReceived += FaultPollingService_OnFaultsUpdated;
                await FaultPollingService.StartAsync();
                return;
            }

            if (mapInitialised && !mapMarkersPlaced)
            {
                if (module == null)
                {
                    return;
                }

                var location = await LocationService.GetCurrentDeviceLocationAsync();

                if (location is not null)
                {
                    homePin = new GoogleMapPin()
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Icon = GoogleMapPinIconConstants.Home,
                        IsFault = false,
                    };

                    //markers.Add(homePin);
                }

                await module.InvokeVoidAsync("initMap", markers, dotNetObjectReference);
                mapMarkersPlaced = true;

                if (location is not null)
                {
                    //await module.InvokeVoidAsync("MoveToLocation", location.Latitude, location.Longitude);
                }
            }

            if (!isCalculatingCountyOutagesStarted)
            {
                await CalculateCountyOutages();
            }
        }

        private async Task<IEnumerable<AddressUI>> GetFavouriteAddresses()
        {
            AuthenticationState authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            ClaimsPrincipal claimsPrincipal = authState.User;

            if (claimsPrincipal.Identity?.IsAuthenticated == true)
            {
                string? userId = claimsPrincipal.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userId is not null)
                {
                    return await AddressService.GetFavouriteAddresses(userId);

                    // if (retrievedAddresses is not null)
                    // {
                    //     this.addresses.AddRange(retrievedAddresses.Select(a => (AddressUI)a));
                    // }
                }
            }

            return null;
        }

        private async Task CalculateCountyOutages()
        {
            isCalculatingCountyOutagesStarted = true;

            foreach (var marker in markers.Where(m => m.IsFault))
            {
                await Task.Run(async () =>
                {
                    string countyName = countyMatcher.MatchCounties(marker.Longitude, marker.Latitude);

                    await InvokeAsync(() =>
                    {
                        if (!countyOutages.ContainsKey(countyName))
                        {
                            return;
                        }

                        if (!countyOutages.TryGetValue(countyName, out int count))
                        {
                            count = 0;
                        }

                        countyOutages[countyName] = count + 1;
                    });
                });
            }

            isCalculatingCountyOutages = false;
            await InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        public void OnMarkerClicked(GoogleMapPin googleMapPin)
        {
            this.googleMapPin = googleMapPin;
            showModal = true;
            StateHasChanged();
        }

        private void CloseModal()
        {
            showModal = false;
        }

        private async Task ZoomIn() => await module.InvokeVoidAsync("zoomMap", 1);
        private async Task ZoomOut() => await module.InvokeVoidAsync("zoomMap", -1);
        private async Task ToggleGeoJson() => await module.InvokeVoidAsync("toggleGeoJson", countyOutages);

        public void SendMessage()
        {
            SmsSender.SendMessage(PersonalSettings.Value.PhoneNumber, "Test message");
        }

        public async void SendEmail()
        {
            await EmailSender.SendEmailAsync(PersonalSettings.Value.Email, "There is a power cut", "There is a disruption to the network");
        }

        public ValueTask DisposeAsync()
        {
            FaultPollingService.OnFaultsReceived -= FaultPollingService_OnFaultsUpdated;
            return ValueTask.CompletedTask;
        }
    }
}
