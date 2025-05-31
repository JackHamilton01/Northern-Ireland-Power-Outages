using NorthernIrelandPowerOutages.Models;

namespace NorthernIrelandPowerOutages.Services
{
    public interface IFaultPollingService
    {
        public FaultModel? CurrentFault { get; }

        event Action<FaultModel?, bool> OnFaultsReceived;

        Task StartAsync(CancellationToken cancellationToken = default);
    }
}