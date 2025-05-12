using Microsoft.AspNetCore.StaticFiles;

namespace NorthernIrelandPowerOutages.Startup
{
    public static class StaticFilesConfig
    {
        public static void AddStaticFiles(this WebApplication app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = new FileExtensionContentTypeProvider
                {
                    Mappings = { [".geojson"] = "application/geo+json" }
                }
            });
        }
    }
}
