using Domain.Backend;
using FaultsAPI.Models;
using GeocodeService;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Helpers;
using Infrastructure.Sms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NorthernIrelandPowerOutages.Models;
using NorthernIrelandPowerOutages.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultService
{
    public class HistoricalFaultSavingService : IHistoricalFaultSavingService
    {
        private readonly IFaultPollingService faultPollingService;
        private readonly IGeocodeService geocodeService;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IEmailSender emailSender;
        private readonly ISmsSender smsSender;

        public HistoricalFaultSavingService(
            IFaultPollingService faultPollingService,
            IGeocodeService geocodeService,
            IServiceScopeFactory scopeFactory,
            IEmailSender emailSender,
            ISmsSender smsSender)
        {
            this.faultPollingService = faultPollingService ?? throw new ArgumentNullException(nameof(faultPollingService));
            this.geocodeService = geocodeService ?? throw new ArgumentNullException(nameof(geocodeService));
            this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            this.smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
        }

        public void StartListeningForFaults()
        {
            faultPollingService.OnFaultsReceived += FaultPollingService_OnFaultsReceived;
        }

        public void StopListeningForFaults()
        {
            faultPollingService.OnFaultsReceived -= FaultPollingService_OnFaultsReceived;
        }

        private async void FaultPollingService_OnFaultsReceived(FaultModel? faults, bool isFirstPoll)
        {
            try
            {
                try
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        ApplicationDbContext? dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        await DeleteOldFaults(scope, dbContext);
                        List<HistoricalFault>? historicalFaults = await SaveNewFaults(faults, dbContext);

                        foreach (Address favouritedAddress in dbContext.Addresses.Include(a => a.FavouriteAddressPreferences).ToList())
                        {
                            foreach (HistoricalFault historicalFault in historicalFaults)
                            {
                                if (favouritedAddress.StreetName == historicalFault.StreetName &&
                                    favouritedAddress.StreetNumber == historicalFault.StreetNumber)
                                {
                                    foreach (FavouriteAddressPreferences preference in favouritedAddress.FavouriteAddressPreferences)
                                    {
                                        ApplicationUser? user = dbContext.Users.FirstOrDefault(u => u.Id == preference.ApplicationUserId);

                                        string powerOutageMessage = $"There has been a power outage at {favouritedAddress.StreetNumber} {favouritedAddress.StreetName}, {favouritedAddress.PostCode}";

                                        if (!preference.AlertSent)
                                        {
                                            if (preference.EmailAlertsEnabled)
                                            {
                                                //await emailSender.SendEmailAsync(
                                                //    user.Email,
                                                //    $"Power Outage - {favouritedAddress.StreetNumber} {favouritedAddress.StreetName}",
                                                //    powerOutageMessage);
                                            }
                                            if (preference.SmsAlertsEnabled)
                                            {
                                                //await smsSender.SendMessageAsync(user.PhoneNumber, powerOutageMessage);
                                            }
                                        }

                                        preference.AlertSent = true;
                                    }
                                }
                                else
                                {
                                    foreach (FavouriteAddressPreferences preference in favouritedAddress.FavouriteAddressPreferences)
                                    {
                                        preference.AlertSent = false;
                                    }

                                    await dbContext.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<List<HistoricalFault>> SaveNewFaults(FaultModel? faults, ApplicationDbContext dbContext)
        {
            List<HistoricalFault> historicalFaults = new();

            foreach (OutageMessage outage in faults.OutageMessage)
            {
                var coordinates = CoordinateHelpers.ConvertIrishGridToLatLon(outage.Point.Easting, outage.Point.Northing);

                GoogleGeocodeResponse? result = await geocodeService.GetAddressFromLatLng(coordinates.Latitude, coordinates.Longitude);

                var res = result.Results.Where(r => !r.Types.Contains("plus_code")).FirstOrDefault();

                //if (res.Types.FirstOrDefault(t => t == "route") == "route")
                //{
                //    continue;
                //}
                var streetNumber = res.AddressComponents
                    .Where(a => a.Types.Contains("street_number"));

                if (streetNumber.ToList() is null || streetNumber.Count() == 0)
                {
                    return historicalFaults;
                }

                HistoricalFault historicalFault = new()
                {
                    Id = 0,
                    StreetNumber = res.AddressComponents.Where(a => a.Types.Contains("street_number"))
                    .FirstOrDefault()!
                    .ShortName,

                    StreetName = res.AddressComponents.Where(a => a.Types.Contains("route"))
                    .FirstOrDefault()!
                    .ShortName!,

                    PostCode = res.AddressComponents.Where(a => a.Types.Contains("postal_code"))
                    .FirstOrDefault()!
                    .ShortName!,
                };
                historicalFaults.Add(historicalFault);

            }
            dbContext.HistoricalFaults.AddRange(historicalFaults);
            await dbContext.SaveChangesAsync();

            return historicalFaults;
        }

        private async Task DeleteOldFaults(IServiceScope scope, ApplicationDbContext dbContext)
        {
            foreach (HistoricalFault historicalFault in dbContext.HistoricalFaults)
            {
                dbContext.HistoricalFaults.Remove(historicalFault);
            }
            await dbContext.SaveChangesAsync();
        }
    }
}
