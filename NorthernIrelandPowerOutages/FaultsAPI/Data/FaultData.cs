using FaultsAPI.Models;
using System.Text.Json;

namespace FaultsAPI.Data
{
    public class FaultData
    {
        public FaultModel Faults { get; private set; }

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

            Faults = JsonSerializer.Deserialize<FaultModel>(json, options) ?? throw new InvalidOperationException("Failed to deserialize faultdata.json into a valid FaultModel."); ;
        }
    }
}
