using Infrastructure.ProjectSettings;

namespace NorthernIrelandPowerOutages.Startup
{
    public static class UserSecretsConfig
    {
        public static void AddUserSecrets(this WebApplicationBuilder builder)
        {
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();

                builder.Services.Configure<TwilioSettings>(
                builder.Configuration.GetSection("Twilio"));

                builder.Services.Configure<SmtpSettings>(
                builder.Configuration.GetSection("Smtp"));

                builder.Services.Configure<PersonalSettings>(
                builder.Configuration.GetSection("Personal"));

                builder.Services.Configure<GoogleMapsSettings>(
                builder.Configuration.GetSection("GoogleMaps"));
            }
        }
    }
}
