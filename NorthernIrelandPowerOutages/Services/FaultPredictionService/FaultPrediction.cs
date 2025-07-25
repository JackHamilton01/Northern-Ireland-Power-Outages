using Domain.Backend;
using Domain.Frontend;
using FaultsAPI.Models;
using Infrastructure.Data;
using Infrastructure.Helpers;
using Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using NorthernIrelandPowerOutages.Models;
using NorthernIrelandPowerOutages.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FaultPredictionService
{
    public class FaultPrediction : IFaultPrediction
    {
        private readonly IFaultPollingService faultPollingService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly HttpClient httpClient;
        private readonly ApplicationDbContext dbContext;
        private readonly Random random = new();
        private int numberOfOutageTraining = 5;

        public FaultPrediction(
            IFaultPollingService faultPollingService,
            IServiceScopeFactory scopeFactory,
            HttpClient httpClient,
            ApplicationDbContext dbContext)
        {
            this.faultPollingService = faultPollingService ?? throw new ArgumentNullException(nameof(faultPollingService));
            this._scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            faultPollingService.OnFaultsReceived += OnFaultsReceived;
        }

        private List<OutagePredictionTrainingData> SelectOutageMessages(IEnumerable<OutageMessage> outageMessages)
        {
            List<OutagePredictionTrainingData> outagePredictionTraining = new();

            foreach (var item in outageMessages)
            {
                var result = CoordinateHelpers.ConvertIrishGridToLatLon(item.Point.Easting, item.Point.Northing);

                outagePredictionTraining.Add(new OutagePredictionTrainingData
                {
                    Latitude = result.Latitude,
                    Longitude = result.Longitude,
                    Outage = 1,
                });
            }

            if (outageMessages == null || !outageMessages.Any())
            {
                return new List<OutagePredictionTrainingData>();
            }

            if (outageMessages.Count() <= numberOfOutageTraining)
            {
                return outagePredictionTraining;
            }
            else
            {
                return outagePredictionTraining
                    .OrderBy(x => random.Next())
                    .Take(numberOfOutageTraining)
                    .ToList();
            }
        }

        private async void OnFaultsReceived(FaultModel? faults, bool isFirstPoll)
        {
            //using (var scope = _scopeFactory.CreateScope()) 
            //{
            //    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //} 

            var faultTrainingData = SelectOutageMessages(faults.OutageMessage);

            List<OutagePredictionTrainingData> noFaultTrainingData = new();
            for (int i = 0; i < faultTrainingData.Count; i++)
            {
                var (latitude, longitude) = GetRandomNILocation();
                noFaultTrainingData.Add(new OutagePredictionTrainingData
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    Outage = 0,
                });
            }

            await Train(faultTrainingData.Concat(noFaultTrainingData));
        }

        public async Task<bool> Train(IEnumerable<OutagePredictionTrainingData> outagePredictionTrainingData)
        {
            OutagesJson outagesJson = new OutagesJson
            {
                Outages = outagePredictionTrainingData.ToList()
            };

            string requestUrl = "http://127.0.0.1:5000/train";

            string jsonContent = JsonSerializer.Serialize(outagesJson, new JsonSerializerOptions { WriteIndented = true });

            StringContent? content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content);

            return response.IsSuccessStatusCode;
        }

        public async Task<List<PredictionUI>?> GetFaultPredictions()
        {
            return await httpClient.GetFromJsonAsync<List<PredictionUI>>("http://127.0.0.1:5000/predicted-outages");
        }

        public (double Latitude, double Longitude) GetRandomNILocation()
        {
            double minLat = 54.0;
            double maxLat = 55.3;
            double minLng = -8.2;
            double maxLng = -5.3;

            double randomLat = minLat + (random.NextDouble() * (maxLat - minLat));
            double randomLng = minLng + (random.NextDouble() * (maxLng - minLng));

            return (randomLat, randomLng);
        }

        private class OutagesJson
        {
            [JsonPropertyName("outages")]
            public List<OutagePredictionTrainingData> Outages { get; set; }
        }
    }
}
