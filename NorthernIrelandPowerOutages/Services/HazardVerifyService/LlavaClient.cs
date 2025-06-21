using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HazardVerifyService
{
    public static class LlavaClient
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:11434")
        };

        public static async Task<HazardImageResult> CallLlavaAsync(string imagePath, string prompt)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Image file not found.", imagePath);
            }

            string base64Image = Convert.ToBase64String(File.ReadAllBytes(imagePath));

            var requestBody = new
            {
                model = "llava:7b",
                prompt = prompt,
                images = new[] { base64Image },
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/generate", content);
            string responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"LLaVA call failed: {response.StatusCode}\n{responseText}");
            }

            var result = JsonSerializer.Deserialize<HazardImageResult>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
    }
}
