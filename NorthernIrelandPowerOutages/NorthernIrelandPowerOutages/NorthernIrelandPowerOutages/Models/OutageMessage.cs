namespace NorthernIrelandPowerOutages.Models
{
    public class OutageMessage
    {
        public required string OutageId { get; set; }
        public required string OutageType { get; set; }
        public required Geopoint Point { get; set; }
        public required string StartTime { get; set; }
        public required string EstRestoreTime { get; set; }
        public required string EstRestoreFullDateTime { get; set; }
        public required string PostCode { get; set; }
        public required string FullPostCodes { get; set; }
        public required string NumCustAffected { get; set; }
        public required string StatusMessage { get; set; }
        public required string CauseMessage { get; set; }
        public required string UpdatedTimeStamp { get; set; }
    }
}
