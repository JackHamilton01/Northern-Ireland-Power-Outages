using NorthernIrelandPowerOutages.Models;
using System.Diagnostics;
using System.Timers;

namespace NorthernIrelandPowerOutages.Services
{
    public class FaultPollingService
    {
        private readonly HttpClient httpClient;

        public event Action<FaultModel?> OnFaultsReceived;

        public FaultPollingService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await GetCurrentFaults();

                var now = DateTime.Now;
                var next5Min = now.AddMinutes(5 - now.Minute % 5).AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond);
                var delay = next5Min - now;

                Debug.WriteLine($"Next poll at {next5Min}");

                await Task.Delay(delay, cancellationToken);
            }
        }

        private async Task GetCurrentFaults()
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<FaultModel>("https://localhost:7125/faults");
                OnFaultsReceived?.Invoke(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Polling error: {ex.Message}");
            }
        }
    }

}
