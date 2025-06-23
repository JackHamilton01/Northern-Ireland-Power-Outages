using Domain.Frontend;

namespace Domain.Backend
{
    public class Address
    {
        public virtual required int Id { get; set; }
        public virtual required int StreetNumber { get; set; }
        public virtual required string StreetName { get; set; }
        public virtual required string? BuildingDetails { get; set; }
        public virtual required string City { get; set; }
        public virtual required string County { get; set; }
        public virtual required string PostCode { get; set; }
        public virtual required double  Longitude { get; set; }
        public virtual required double Latitude { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }

        public static implicit operator AddressUI(Address address)
        {
            return new AddressUI
            {
                Id = address.Id,
                StreetNumber = address.StreetNumber,
                StreetName = address.StreetName,
                BuildingDetails = address.BuildingDetails,
                City = address.City,
                County = address.County,
                PostCode = address.PostCode,
                Latitude = address.Latitude,
                Longitude = address.Longitude
            };
        }
    }
}