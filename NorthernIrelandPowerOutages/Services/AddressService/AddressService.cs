using Domain.Backend;
using Domain.Frontend;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace AddressService
{
    public class AddressService : IAddressService
    {
        private readonly HttpClient httpClient;

        public AddressService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<AddressUI> GetAddressByIdAsync(int id)
        {
            var addressUI = await httpClient.GetFromJsonAsync<AddressUI>($"https://localhost:7228/address/{id}/addresses");

            if (addressUI == null)
            {
                throw new InvalidOperationException($"No address found for ID {id}");
            }

            return addressUI;
        }

        public async Task<AddressUI?> FindMatchingAddress(AddressUI inputAddress)
        {
            var response = await httpClient.PostAsJsonAsync("https://localhost:7228/address/match", inputAddress);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AddressUI>();
            }
            else
            {
                throw new Exception("Request failed");
            }
        }

        public async Task<IEnumerable<AddressUI>>? GetFavouriteAddresses(string userId)
        {
            return await httpClient.GetFromJsonAsync<List<AddressUI>?>(
                $"https://localhost:7228/address/{userId}/addresses");
        }
    }
}
