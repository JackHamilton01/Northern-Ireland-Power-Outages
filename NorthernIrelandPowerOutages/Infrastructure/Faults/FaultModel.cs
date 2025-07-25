using FaultsAPI.Models;

namespace NorthernIrelandPowerOutages.Models
{
    public class FaultModel
    {
        public required EscalationInfoModel EscalationInfo { get; set; }
        public required OutageMessage[] OutageMessage { get; set; }
    }
}
