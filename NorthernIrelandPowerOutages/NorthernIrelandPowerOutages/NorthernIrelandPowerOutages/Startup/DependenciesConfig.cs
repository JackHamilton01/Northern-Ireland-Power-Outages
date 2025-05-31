using EmailService;
using Infrastructure.Email;
using Infrastructure.ProjectSettings;
using Infrastructure.Sms;
using LocationService;
using NorthernIrelandPowerOutages.Services;
using SmsService;

namespace NorthernIrelandPowerOutages.Startup
{
    public static class DependenciesConfig
    {
        public static void AddDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ISmsSender, SmsSender>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<ILocationService, LocationService.LocationService>();

            builder.Services.AddSingleton<IFaultPollingService, FaultPollingService>();

            builder.Services.AddHttpClient();

            builder.Services.Configure<FaultsApiSettings>(
                builder.Configuration.GetSection("FaultsApiSettings"));

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization();
        }
    }
}
