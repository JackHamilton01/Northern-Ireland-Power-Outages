using FaultsAPI.Models;
using System.Net.Http;
using System.Text.Json;

namespace FaultsAPI.Data
{
    public class FaultData
    {
        private readonly HttpClient httpClient;
        private string faultsUrl = "https://powercheck.nienetworks.co.uk/NIEPowerCheckerWebAPI/api/faults";

        public FaultData(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<FaultModel> LoadFaultsAsync()
        {
            string json = await httpClient.GetStringAsync(faultsUrl);

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };

            return JsonSerializer.Deserialize<FaultModel>(json, options)
                ?? throw new InvalidOperationException("Failed to deserialize JSON into a valid FaultModel.");
        }

        private FaultModel LoadFaultDataFromUrl()
        {
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };

            string filePath = Path.Combine(Directory.GetCurrentDirectory(),
                "Data",
                "faultdata.json");

            string json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<FaultModel>(json, options) ?? throw new InvalidOperationException("Failed to deserialize faultdata.json into a valid FaultModel.");
        }

        private FaultModel LoadFaultDataFromJson()
        {
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };

            string filePath = Path.Combine(Directory.GetCurrentDirectory(),
                "Data",
                "faultdata.json");

            string json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<FaultModel>(json, options) ?? throw new InvalidOperationException("Failed to deserialize faultdata.json into a valid FaultModel.");
        }
    }
}
