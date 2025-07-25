using Domain.Backend;
using Domain.Frontend;
using FaultPredictionService;
using FaultsAPI.Data;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.CodeAnalysis;
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
        private bool showUploadSelectOverlay = false;
        private bool showUploadServiceOverlay = false;
        private OutageMessage? selectedFault;
        private bool showFaultOverlay = false;
        private bool showPlannedOutageOverlay = false;
        private List<GoogleMapPin>? childPins;

        private double newMarkerLatitude;
        private double newMarkerLongitude;

        private void HideHazardUploadOverlay() => showUploadHazardOverlay = false;
        private void HideHazardViewOverlay() => showHazardViewOverlay = false;

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
                        Name = outageMessage.OutageId,
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
            await GetAllServicesAndDisplay();
            await GetAllFaultPredictions();
            await HandleApproximateMarkerLocations();
        }

        private async Task GetAllServicesAndDisplay()
        {
            var services = await Http.GetFromJsonAsync<List<ServiceUI>>("https://localhost:7228/services");

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };
            DisplayServices(services);
        }

        private async Task GetAllFaultPredictions()
        {
            try
            {
                List<PredictionUI>? predictions = await faultPrediction.GetFaultPredictions();

                if (predictions is not null)
                {
                    foreach (var prediction in predictions)
                    {
                        // Check if there's already a marker close to this prediction
                        bool isClose = markers.Any(m =>
                            Math.Abs(m.Latitude - prediction.Latitude) < 0.01 &&
                            Math.Abs(m.Longitude - prediction.Longitude) < 0.01);

                        if (!isClose)
                        {
                            markers.Add(new GoogleMapPin()
                            {
                                Name = "Prediction",
                                Latitude = prediction.Latitude,
                                Longitude = prediction.Longitude,
                                IsFault = false,
                                MarkerType = MarkerType.Prediction,
                                Icon = GoogleMapPinIconConstants.Prediction,
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
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
                    dedupedMarkers.Add(pinsAtLocation[0]);
                }
                else
                {
                    var representativeLatitude = pinsAtLocation.Average(p => p.Latitude);
                    var representativeLongitude = pinsAtLocation.Average(p => p.Longitude);

                    var multiplePin = new GoogleMapPin
                    {
                        Name = "Multiple",
                        StatusMessage = $"{pinsAtLocation.Count} items", 
                        Latitude = representativeLatitude,
                        Longitude = representativeLongitude,
                        IsFault = false, 
                        MarkerType = MarkerType.Multiple,
                        Icon = GoogleMapPinIconConstants.Multiple,
                        ChildPins = pinsAtLocation
                    };
                    dedupedMarkers.Add(multiplePin);
                }
            }

            markers = dedupedMarkers;
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

                HistoricalFaultSavingService.StartListeningForFaults();
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
            try
            {
                this.googleMapPin = googleMapPin;
                await ShowOverlay(googleMapPin);

                StateHasChanged();
            }
            catch (Exception ex)
            {
                
            }
        }

        private async Task ShowOverlay(GoogleMapPin googleMapPin)
        {
            if (googleMapPin is null)
            {
                CloseModal();
                return;
            }

            if (googleMapPin.MarkerType == MarkerType.Fault)
            {
                var outageId = googleMapPin.Name;
                selectedFault = await Http.GetFromJsonAsync<OutageMessage>($"https://localhost:7125/faults/{outageId}");

                showFaultOverlay = true;
            }
            else if (googleMapPin.MarkerType == MarkerType.Planned)
            {
                var outageId = googleMapPin.Name;
                string encodedString = Uri.EscapeDataString(outageId);
                selectedFault = await Http.GetFromJsonAsync<OutageMessage>($"https://localhost:7125/faults/{encodedString}");

                showPlannedOutageOverlay = true;
            }
            else if (googleMapPin.MarkerType == MarkerType.Multiple)
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
            SmsSender.SendMessageAsync(PersonalSettings.Value.PhoneNumber, "Test message");
        }

        public async void SendEmail()
        {
            await EmailSender.SendEmailAsync(PersonalSettings.Value.Email, "There is a power cut", "There is a disruption to the network");
        }

        private async Task GetAllHazardsAndDisplay()
        {
            var hazards = await Http.GetFromJsonAsync<List<HazardUI>>("https://localhost:7228/hazards");

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };
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

        private void DisplayServices(List<ServiceUI> services)
        {
            if (services is not null)
            {
                foreach (ServiceUI service in services)
                {
                    markers.Add(new GoogleMapPin()
                    {
                        Name = service.Name,
                        Latitude = service.Latitude,
                        Longitude = service.Longitude,
                        IsFault = false,
                        MarkerType = MarkerType.Service,
                        Icon = GoogleMapPinIconConstants.Service,
                    });
                }
            }
        }

        private async Task HideMultiplePinOverlay(GoogleMapPin googleMapPin)
        {
            showMultiplePinOverlay = false;

            await ShowOverlay(googleMapPin);
        }

        private void HandleMarkerTypeSelected(UploadType uploadType)
        {
            if (uploadType == UploadType.Hazard)
            {
                showUploadHazardOverlay = true;
            }
            else if (uploadType == UploadType.Service)
            {
                showUploadServiceOverlay = true;
            }

            HideUploadSelectOverlay();
        }

        private void HideUploadSelectOverlay()
        {
            showUploadSelectOverlay = false;
        }

        private void HideServiceUploadOverlay()
        {
            showUploadServiceOverlay = false;
        }

        private void HideFaultOverlay()
        {
            showFaultOverlay = false;
            selectedFault = null;
        }

        private void HidePlannedOutageOverlay()
        {
            showPlannedOutageOverlay = false;
            selectedFault = null;
        }

        private async Task OnSearchResultSuccess(Tuple<double, double> geopoint)
        {
            int zoomLevel = 12; // Default zoom level for search results
            await module.InvokeVoidAsync("MoveToLocation", geopoint.Item1, geopoint.Item2, zoomLevel);

            GoogleMapPin? marker = markers.FirstOrDefault(m => m.Latitude == geopoint.Item1 && m.Longitude == geopoint.Item2);
            if (marker is not null)
            {
                await ShowOverlay(marker);
            }
        }

        [JSInvokable]
        public async Task HandleMapClick(double latitude, double longitude)
        {
            newMarkerLatitude = latitude;
            newMarkerLongitude = longitude;

            showUploadSelectOverlay = true;

            StateHasChanged();

            isPlacingMarkers = !isPlacingMarkers;
            await module.InvokeVoidAsync("toggleMapClickListener", isPlacingMarkers);
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