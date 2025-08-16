using NorthernIrelandPowerOutages.Models;
using NorthernIrelandPowerOutages.Services;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Timers;

namespace FaultService
{
    public class FaultPollingService : IFaultPollingService
    {
        private CancellationTokenSource cancellationTokenSource;

        private readonly HttpClient httpClient;
        private bool isFirstPoll = true;

        public FaultModel? CurrentFault { get; set; }

        public event Action<FaultModel?, bool> OnFaultsReceived;

        public FaultPollingService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await GetCurrentFaults();
                isFirstPoll = false;

                var now = DateTime.Now;
                var next5Min = now.AddMinutes(5 - now.Minute % 5).AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond);
                var delay = next5Min - now;

                Debug.WriteLine($"Next poll at {next5Min}");

                await Task.Delay(delay, cancellationToken);
            }
        }

        public async Task Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private async Task GetCurrentFaults()
        {
            FaultModel? response = await httpClient.GetFromJsonAsync<FaultModel>("https://localhost:7125/faults");

            CurrentFault = response;

            OnFaultsReceived?.Invoke(response, isFirstPoll);
        }
    }
}
