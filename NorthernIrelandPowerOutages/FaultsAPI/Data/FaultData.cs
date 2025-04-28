using FaultsAPI.Models;
using System.Text.Json;

namespace FaultsAPI.Data
{
    public class FaultData
    {
        public List<FaultModel> Faults { get; private set; }

        public FaultData()
        {
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), 
                "Data",
                "faultdata.json");

            string json = File.ReadAllText(filePath);

            Faults = JsonSerializer.Deserialize<List<FaultModel>>(json, options) ?? new();
        }
    }
}
