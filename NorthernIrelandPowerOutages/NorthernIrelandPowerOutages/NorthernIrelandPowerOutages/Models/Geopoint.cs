using Newtonsoft.Json;

namespace NorthernIrelandPowerOutages.Models
{
    public class Geopoint
    {
        private string coordinates;
        public string Coordinates
        {
            get => coordinates;
            set
            {
                coordinates = value;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    var parts = value.Split(',');

                    if (parts.Length == 2)
                    {
                        if (double.TryParse(parts[0], out var easting))
                        {
                            Easting = easting;
                        }

                        if (double.TryParse(parts[1], out var northing))
                        {
                            Northing = northing;
                        }
                    }
                }
            }
        }

        [JsonIgnore]
        public double Easting { get; private set; }

        [JsonIgnore]
        public double Northing { get; private set; }
    }
}
