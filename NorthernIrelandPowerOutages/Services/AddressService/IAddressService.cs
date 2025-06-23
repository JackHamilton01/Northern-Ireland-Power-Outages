using Domain.Backend;
using Domain.Frontend;

namespace AddressService
{
    public interface IAddressService
    {
        Task<AddressUI> GetAddressByIdAsync(int id);
        Task<AddressUI?> FindMatchingAddress(AddressUI inputAddress);
        Task<IEnumerable<AddressUI>>? GetFavouriteAddresses(string userId);
    }
}