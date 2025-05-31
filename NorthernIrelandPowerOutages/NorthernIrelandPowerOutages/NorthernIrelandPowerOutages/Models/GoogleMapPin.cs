using NorthernIrelandPowerOutages.Enums;

namespace NorthernIrelandPowerOutages.Models
{
    public class GoogleMapPin
    {
        public string Name { get; set; }
        public string StatusMessage { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Size { get; set; }
        public string Icon { get; set; }
    }
}
