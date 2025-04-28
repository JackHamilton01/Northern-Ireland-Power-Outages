using System.Text.Json.Serialization;

namespace NorthernIrelandPowerOutages.Client.Models
{
    public class FaultModel
    {
        public required string IncidentReference { get; set; }
        public required string PowerCutType { get; set; }
        public required DateTime CreationDateTime { get; set; }
        public required int NoCallsReported { get; set; }
        public required int IncidentsCount { get; set; }
        public required int NoCustomerAffected { get; set; }
        public required string PostCodesAffected { get; set; }
        public required string RestoredIncidents { get; set; }
        public required string UnplannedIncidents { get; set; }
        public required string PlannedIncidents { get; set; }
        public required string IncidentTypeTBCEstimatedFriendlyDescription { get; set; }
        public required string IncidentDescription { get; set; }
        public required string FullPostcodeData { get; set; }
        public required string IncidentCategoryCustomerFriendlyDescription { get; set; }
        public int? IncidentCategory { get; set; }
        public required string IncidentTypeName { get; set; }
        public required int IncidentType { get; set; }
        public required int IncidentPriority { get; set; }
        public required int StatusId { get; set; }
        public required string RestoredDateTime { get; set; }

        public DateTime? PlannedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }

        public required int NoPlannedCustomers { get; set; }
        public required string PlannedIncidentReason { get; set; }
        public required string Message { get; set; }
        public required string MainMessage { get; set; }
        public required string Geopoint { get; set; }

        public DateTime? EstimatedRestorationDate { get; set; }
    }
}
