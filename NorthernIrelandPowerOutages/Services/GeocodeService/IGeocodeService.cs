
namespace GeocodeService
{
    public interface IGeocodeService
    {
        Task<Location?> GetLatLongFromAddressAsync(string address);
        Task<List<string>> GetAddressesFromPostcodeAsync(string postcode);
        Task<GoogleGeocodeResponse> GetAddressFromLatLng(double latitude, double longitude);
    }
}