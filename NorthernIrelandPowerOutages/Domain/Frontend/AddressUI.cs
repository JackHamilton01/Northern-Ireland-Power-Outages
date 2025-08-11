using Domain.Backend;

namespace Domain.Frontend
{
    public class AddressUI
    {
        public int Id { get; set; }
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string? BuildingDetails { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool EmailAlertsEnabled { get; set; }
        public bool SmsAlertsEnabled { get; set; }

        public AddressUI Clone()
        {
            return new AddressUI
            {
                Id = this.Id,
                StreetNumber = this.StreetNumber,
                StreetName = this.StreetName,
                BuildingDetails = this.BuildingDetails,
                City = this.City,
                County = this.County,
                PostCode = this.PostCode,
                Latitude = this.Latitude,
                Longitude = this.Longitude,
            };
        }

        public static implicit operator Address(AddressUI ui)
        {
            return new Address
            {
                Id = ui.Id,
                StreetNumber = ui.StreetNumber,
                StreetName = ui.StreetName,
                BuildingDetails = ui.BuildingDetails,
                City = ui.City,
                County = ui.County,
                PostCode = ui.PostCode,
                Latitude = ui.Latitude,
                Longitude = ui.Longitude,
            };
        }

        public override string ToString()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(StreetNumber) && 
                !string.IsNullOrWhiteSpace(StreetName))
            {
                parts.Add($"{StreetNumber} {StreetName}");
            }
            else if (!string.IsNullOrWhiteSpace(StreetName))
            {
                parts.Add(StreetName);
            }

            if (!string.IsNullOrWhiteSpace(BuildingDetails))
            {
                parts.Add(BuildingDetails);
            }

            if (!string.IsNullOrWhiteSpace(City))
            {
                parts.Add(City);
            }

            if (!string.IsNullOrWhiteSpace(County))
            {
                parts.Add(County);
            }

            if (!string.IsNullOrWhiteSpace(PostCode))
            {
                parts.Add(PostCode);
            }

            parts.Add("Northern Ireland");

            return string.Join(", ", parts);
        }
    }
}