
namespace LocationService
{
    public interface ILocationService
    {
        Task<Location?> GetCurrentDeviceLocationAsync();
    }
}