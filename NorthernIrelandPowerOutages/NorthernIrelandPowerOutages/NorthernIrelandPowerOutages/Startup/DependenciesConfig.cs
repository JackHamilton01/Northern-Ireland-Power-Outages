﻿using AddressService;
using EmailService;
using GeocodeService;
using HazardVerifyService;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.ProjectSettings;
using Infrastructure.Sms;
using LocationService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using NorthernIrelandPowerOutages.Components.Account;
using NorthernIrelandPowerOutages.Services;
using SmsService;

namespace NorthernIrelandPowerOutages.Startup
{
    public static class DependenciesConfig
    {
        public static void AddDependencies(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("Connection string 'Postgres' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddHttpClient();

            builder.Services.AddScoped<ISmsSender, SmsSender>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IDeviceLocationService, DeviceLocationService>();

            builder.Services.AddHttpClient<IAddressService, AddressService.AddressService>(client =>
            {
            });


            builder.Services.AddScoped<IGeocodeService, GeocodeService.GeocodeService>();

            builder.Services.AddSingleton<IFaultPollingService, FaultPollingService>();

            builder.Services.AddChatClient(HazardVerify.GetOllamaApiClient());

            builder.Services.Configure<FaultsApiSettings>(
                builder.Configuration.GetSection("FaultsApiSettings"));

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        }
    }
}
