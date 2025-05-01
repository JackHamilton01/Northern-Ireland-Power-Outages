using System.Text.Json.Serialization;

namespace FaultsAPI.Models
{
    public class FaultModel
    {
        public required EscalationInfoModel Escalationinfo { get; set; }
        public required OutageMessage[] OutageMessage { get; set; }
    }
}
