using OllamaSharp;

namespace HazardVerifyService
{
    public static class HazardVerify
    {
        public static OllamaApiClient GetOllamaApiClient()
        {
            return new OllamaApiClient(new Uri("http://localhost:11434/"), "llava:7b");
        }
    }
}
