using Microsoft.JSInterop;

namespace LocationService
{
    public class DeviceLocationService : IDeviceLocationService
    {
        private readonly IJSRuntime jsRuntime;

        public DeviceLocationService(IJSRuntime jsRuntime)
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
