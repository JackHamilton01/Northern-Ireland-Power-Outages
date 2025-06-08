
namespace GeocodeService
{
    public interface IGeocodeService
    {
        Task<Location?> GetLatLongFromAddressAsync(string address);
        Task<List<string>> GetAddressesFromPostcodeAsync(string postcode);
    }
}