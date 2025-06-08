using Infrastructure.ProjectSettings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GeocodeService
{
    public partial class GeocodeService : IGeocodeService
    {
        private readonly GoogleMapsSettings settings;

        public GeocodeService(IOptions<GoogleMapsSettings> options)
        {
            settings = options.Value;
        }

        public async Task<Location?> GetLatLongFromAddressAsync(string address)
        {
            var apiKey = settings.ApiKey;
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GoogleGeocodeResponse>(json, options);

            // Use street_address to get the most accurate coordinates
            return result?.Results?.FirstOrDefault(r => r.Types.FirstOrDefault() == "street_address")?.Geometry?.Location;
        }

        public async Task<List<string>> GetAddressesFromPostcodeAsync(string postcode)
        {
            var apiKey = settings.ApiKey; // Replace with your Google API key
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(postcode)}&key={apiKey}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new List<string>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var geocodeResponse = JsonSerializer.Deserialize<GoogleGeocodeResponse>(json);

            if (geocodeResponse?.Results == null)
            {
                return new List<string>();
            }

            return geocodeResponse.Results
                .Select(r => r.FormattedAddress)
                .Where(addr => !string.IsNullOrEmpty(addr))
                .ToList();
        }
    }
}
