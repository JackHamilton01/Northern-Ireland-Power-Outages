
namespace LocationService
{
    public interface IDeviceLocationService
    {
        Task<Location?> GetCurrentDeviceLocationAsync();
    }
}