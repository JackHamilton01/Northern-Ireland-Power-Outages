using Microsoft.JSInterop;

namespace LocationService
{
    public class LocationService : ILocationService
    {
        private readonly IJSRuntime jsRuntime;

        public LocationService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        public async Task<Location?> GetCurrentDeviceLocationAsync()
        {
            try
            {
                var result = await jsRuntime.InvokeAsync<Location>("getLocation");
                return result;
            }
            catch (JSException jsEx)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
