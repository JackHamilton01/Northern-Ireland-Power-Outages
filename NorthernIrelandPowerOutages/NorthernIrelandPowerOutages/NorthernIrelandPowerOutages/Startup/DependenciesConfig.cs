using EmailService;
using Infrastructure.Email;
using Infrastructure.Sms;
using SMSMessaging;

namespace NorthernIrelandPowerOutages.Startup
{
    public static class DependenciesConfig
    {
        public static void AddDependencies(this WebApplicationBuilder webApplicationBuilder) 
        {
            webApplicationBuilder.Services.AddScoped<ISmsSender, SmsSender>();
            webApplicationBuilder.Services.AddScoped<IEmailSender, EmailSender>();
        }
    }
}
