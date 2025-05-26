using FaultsAPI.Models;

namespace NorthernIrelandPowerOutages.Models
{
    public class FaultModel
    {
        public required EscalationInfoModel escalationInfo { get; set; }
        public required OutageMessage[] OutageMessage { get; set; }
    }
}
