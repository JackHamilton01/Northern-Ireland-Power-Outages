namespace FaultService
{
    public interface IHistoricalFaultSavingService
    {
        void StartListeningForFaults();
        void StopListeningForFaults();
    }
}