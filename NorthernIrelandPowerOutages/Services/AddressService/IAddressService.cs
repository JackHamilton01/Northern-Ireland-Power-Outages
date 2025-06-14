using Domain.Backend;
using Domain.Frontend;

namespace AddressService
{
    public interface IAddressService
    {
        Task<Address> GetAddressByIdAsync(int id);
        Task<Address?> GetMatchAsync(AddressUI inputAddress);
        Task<IEnumerable<Address>> GetFavouriteAddresses(string userId);
    }
}