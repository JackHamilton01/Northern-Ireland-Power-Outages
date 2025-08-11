using Microsoft.AspNetCore.Components;

namespace NorthernIrelandPowerOutages.Components.Overlays
{
    public partial class FavouriteOverlay
    {
        private string location;
        private int outageLikelihood;

        private void CloseOverlay() => OnClose.InvokeAsync();

        [Parameter]
        public EventCallback OnClose { get; set; }

        [Parameter]
        public double Latitude { get; set; }

        [Parameter]
        public double Longitude { get; set; }

        protected override async Task OnInitializedAsync()
        {
            GoogleGeocodeResponse? result = await GeocodeService.GetAddressFromLatLng(Latitude, Longitude);

            Result? address = result.Results.Where(r => !r.Types.Contains("plus_code")).FirstOrDefault();

            location = $"{address.AddressComponents[0].ShortName} \n {address.AddressComponents[1].ShortName}";

            await GetPredictionPercentage();
            base.OnInitialized();
        }

        private async Task GetPredictionPercentage()
        {
            double predictions = await faultPrediction.GetPrediction(Latitude, Longitude);
            outageLikelihood = Convert.ToInt32(predictions * 100);
        }
    }
}