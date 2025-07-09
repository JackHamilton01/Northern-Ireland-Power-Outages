using Domain.Frontend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FaultPredictionService
{
    public class FaultPredictionAPI
    {
        private readonly HttpClient httpClient;

        public FaultPredictionAPI(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<PredictionUI>?> GetFaultPredictions()
        {
            return await httpClient.GetFromJsonAsync<List<PredictionUI>>("http://127.0.0.1:5000/predicted-outages");
        }
    }
}
