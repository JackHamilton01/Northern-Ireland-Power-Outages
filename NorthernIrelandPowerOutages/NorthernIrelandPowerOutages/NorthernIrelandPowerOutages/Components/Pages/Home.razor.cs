using Domain.Backend;
using Domain.Frontend;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using NorthernIrelandPowerOutages.Counties;
using NorthernIrelandPowerOutages.Enums;
using NorthernIrelandPowerOutages.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using Twilio.Http;

namespace NorthernIrelandPowerOutages.Components.Pages
{
    public partial class Home : ComponentBase
    {
        private IJSObjectReference? module;
        private DotNetObjectReference<Home> dotNetObjectReference;
        private int zoomLevel = 8;

        private bool showModal = false;
        private bool isCalculatingCountyOutages = true;
        private bool isPlacingMarkers;

        private bool isCalculatingCountyOutagesStarted = false;
        private bool mapInitialised;
        private bool mapMarkersPlaced = false;

        private GoogleMapPin? googleMapPin;
        private CountyMatcher countyMatcher;
        private GoogleMapPin? homePin;

        private bool showUploadHazardOverlay = false;
        private bool showHazardViewOverlay = false;
        private bool showMultiplePinOverlay = false;
        private List<GoogleMapPin>? childPins;

        private void HideHazardUploadOverlay() => showUploadHazardOverlay = false;
        private void HideHazardViewOverlay() => showHazardViewOverlay = false;

        private double NewHazardLatitude;
        private double NewHazardLongitude;

        private HazardUI activeHazard;

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

            mapInitialised = true;
        }

        private async Task DisplayFaults(FaultModel? faults)
        {
            if (faults is null)
            {
                return;
            }

            markers.Clear();

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
                        MarkerType = outageMessage.OutageType == "Planned" ? MarkerType.Planned : MarkerType.Fault,
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
                        MarkerType = MarkerType.Favourite,
                    });
                }
            }

            await GetAllHazardsAndDisplay();
            await HandleApproximateMarkerLocations();
        }

        private async Task HandleApproximateMarkerLocations(int decimalPlacesForProximity = 4) // Default to 4 decimal places (~11 meters)
        {
            var groupedPins = markers
                .GroupBy(m => new
                {
                    LatitudeBucket = Math.Round(m.Latitude, decimalPlacesForProximity),
                    LongitudeBucket = Math.Round(m.Longitude, decimalPlacesForProximity)
                })
                .ToList();

            List<GoogleMapPin> dedupedMarkers = new();

            foreach (var group in groupedPins)
            {
                var pinsAtLocation = group.ToList();
                if (pinsAtLocation.Count == 1)
                {
                    // Only one pin in this "bucket", add as is
                    dedupedMarkers.Add(pinsAtLocation[0]);
                }
                else
                {
                    // Multiple pins in this "bucket", create a single "Multiple" pin
                    // You might want to calculate the centroid for the representative pin's location
                    var representativeLatitude = pinsAtLocation.Average(p => p.Latitude);
                    var representativeLongitude = pinsAtLocation.Average(p => p.Longitude);

                    var multiplePin = new GoogleMapPin
                    {
                        Name = "Multiple",
                        StatusMessage = $"{pinsAtLocation.Count} items", // Optional: show count
                        Latitude = representativeLatitude,
                        Longitude = representativeLongitude,
                        IsFault = false, // Or derive this from child pins
                        MarkerType = MarkerType.Multiple,
                        Icon = GoogleMapPinIconConstants.Multiple, // Ensure you have this icon
                        ChildPins = pinsAtLocation // Store all pins in this cluster
                    };
                    dedupedMarkers.Add(multiplePin);
                }
            }

            // Replace the original markers list with the deduplicated one
            markers = dedupedMarkers;

            // Trigger UI refresh if this method is called outside of StateHasChanged
            // StateHasChanged();
        }

        private async void FaultPollingService_OnFaultsUpdated(FaultModel? faultData, bool isFirstPoll)
        {
            await DisplayFaults(faultData);

            if (IsHomePage)
            {
                await module.InvokeVoidAsync("updateMarkers", markers);
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
                dotNetObjectReference = DotNetObjectReference.Create(this);

                module = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Home.razor.js");
                await module.InvokeVoidAsync("setBlazorComponentReference", dotNetObjectReference);

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
                        MarkerType = MarkerType.Home,
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
        public async Task OnMarkerClicked(GoogleMapPin googleMapPin)
        {
            this.googleMapPin = googleMapPin;
            await ShowOverlay(googleMapPin);

            StateHasChanged();
        }

        private async Task ShowOverlay(GoogleMapPin googleMapPin)
        {
            if (googleMapPin.MarkerType == MarkerType.Multiple)
            {
                showMultiplePinOverlay = true;
                childPins = googleMapPin.ChildPins;
            }
            else if (googleMapPin.MarkerType == MarkerType.Hazard)
            {
                var geoPoint = new[] { googleMapPin.Latitude, googleMapPin.Longitude };
                var response = await Http.PostAsJsonAsync("https://localhost:7228/hazards/location", geoPoint);

                if (response.IsSuccessStatusCode)
                {
                    activeHazard = await response.Content.ReadFromJsonAsync<HazardUI>();
                }
                else
                {
                    throw new Exception("Request failed");
                }

                showHazardViewOverlay = true;
            }
            else
            {
                showModal = true;
            }
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

        private async Task GetAllHazardsAndDisplay()
        {
            var hazards = await Http.GetFromJsonAsync<List<HazardUI>>("https://localhost:7228/hazards");

            //if (string.IsNullOrWhiteSpace(json))
            //{
            //    return;
            //}

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };

            //List<HazardUI>? hazards = JsonSerializer.Deserialize<List<HazardUI>>(json, options)
            //    ?? throw new InvalidOperationException("Failed to deserialize JSON into a valid list of HazardUI.");

            DisplayHazards(hazards);
        }

        private void DisplayHazards(List<HazardUI> hazards)
        {
            if (hazards is not null)
            {
                foreach (var hazard in hazards)
                {
                    markers.Add(new GoogleMapPin()
                    {
                        Name = hazard.Title,
                        Latitude = hazard.Latitude,
                        Longitude = hazard.Longitude,
                        IsFault = false,
                        MarkerType = MarkerType.Hazard,
                        Icon = GoogleMapPinIconConstants.Hazard,
                    });
                }
            }
        }

        private async Task HideMultiplePinOverlay(GoogleMapPin googleMapPin)
        {
            showMultiplePinOverlay = false;

            await ShowOverlay(googleMapPin);
        }

        [JSInvokable]
        public async Task HandleMapClick(double latitude, double longitude)
        {
            NewHazardLatitude = latitude;
            NewHazardLongitude = longitude;

            showUploadHazardOverlay = true;
            StateHasChanged();

            isPlacingMarkers = !isPlacingMarkers;
            await module.InvokeVoidAsync("toggleMapClickListener", isPlacingMarkers);
            //// Only place a marker if IsPlacingMarkers is true
            //if (isPlacingMarkers)
            //{
            //    await module.InvokeVoidAsync("placeMarker", lat, lng, $"Marker at {lat:F4}, {lng:F4}");
            //    // You could add logic here to store coordinates, etc.
            //}
        }

        private async Task ToggleMarkerPlacement(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            isPlacingMarkers = !isPlacingMarkers;
            await module.InvokeVoidAsync("toggleMapClickListener", isPlacingMarkers);
        }

        public ValueTask DisposeAsync()
        {
            FaultPollingService.OnFaultsReceived -= FaultPollingService_OnFaultsUpdated;
            return ValueTask.CompletedTask;
        }
    }
}
