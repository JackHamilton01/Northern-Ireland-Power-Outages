using Domain.Frontend;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NorthernIrelandPowerOutages.Models;
using static System.Net.WebRequestMethods;

namespace NorthernIrelandPowerOutages.Components.Overlays
{
    public partial class MultiplePinOverlay
    {
        [Parameter]
        public EventCallback<GoogleMapPin> OnClose { get; set; }

        [Parameter]
        public List<GoogleMapPin> ChildPins { get; set; }

        private void CloseOverlay()
        {
            OnClose.InvokeAsync();
        }

        private async Task OnButtonClicked(GoogleMapPin googleMapPin)
        {
            await OnClose.InvokeAsync(googleMapPin);

            //if (googleMapPin.MarkerType is MarkerType.Hazard)
            //{
            //    var geoPoint = new[] { googleMapPin.Latitude, googleMapPin.Longitude };
            //    var response = await Http.PostAsJsonAsync("https://localhost:7228/hazards/location", geoPoint);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        activeHazard = await response.Content.ReadFromJsonAsync<HazardUI>();
            //    }
            //    else
            //    {
            //        throw new Exception("Request failed");
            //    }

            //    showHazardViewOverlay = true;
            //}
        }
    }
}