using Domain.Backend;

namespace Domain.Frontend
{
    public class AddressUI
    {
        public int Id { get; set; }
        public int StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string? BuildingDetails { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }

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
                PostCode = this.PostCode
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
                Latitude = 0,
                Longitude = 0
            };
        }

        public override string ToString()
        {
            var parts = new List<string>();

            if (StreetNumber > 0 && !string.IsNullOrWhiteSpace(StreetName))
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