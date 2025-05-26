using Infrastructure.ProjectSettings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FaultPollerService
{
    public class FaultPollerService : BackgroundService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly FaultsApiSettings faultsApiSettings;

        public FaultPollerService(IHttpClientFactory httpClientFactory, IOptions<FaultsApiSettings> apiSettings)
        {
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.faultsApiSettings = apiSettings?.Value ?? throw new ArgumentNullException(nameof(apiSettings));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            HttpClient? client = httpClientFactory.CreateClient();

            HttpResponseMessage? response = await client.GetAsync(faultsApiSettings.ApiUrl);
            string? data = await response.Content.ReadAsStringAsync();
        }
    }
}
