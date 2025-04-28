using FaultsAPI.Data;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FaultsAPI.EndPoints
{
    public static class FaultEndpoints
    {
        public static void AddFaultEndpoints(this WebApplication app)
        {
            app.MapGet("/faults", LoadAllFaultsAsync);
            app.MapGet("/faults/{incidentReference}", LoadFaultByIncidentReferenceAsync);
        }

        private static async  Task<IResult> LoadAllFaultsAsync(FaultData data, string? powerCutType, string? search, int? delay)
        {
            var output = data.Faults;

            if (powerCutType != null)
            {
                if (!Enum.TryParse<PowerCutType>(powerCutType, true, out var parsedPowerCutType))
                {
                    return Results.BadRequest($"Invalid power cut type: {powerCutType}");
                }

                output.RemoveAll(x => x.PowerCutType != powerCutType);
            }

            if (string.IsNullOrWhiteSpace(search) == false)
            {
                output.RemoveAll(x => !x.FullPostcodeData.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (delay is not null)
            {
                // Max delay of 5 minutes (300,000 milliseconds)
                if (delay > 300000)
                {
                    delay = 300000;
                }

                await Task.Delay((int)delay);
            }

            return Results.Ok(output);
        }

        private static async Task<IResult> LoadFaultByIncidentReferenceAsync(FaultData data, string incidentReference, int? delay)
        {
            var output = data.Faults.SingleOrDefault(x => x.IncidentReference == incidentReference);

            if (delay is not null)
            {
                // Max delay of 5 minutes (300,000 milliseconds)
                if (delay > 300000)
                {
                    delay = 300000;
                }

                await Task.Delay((int)delay);
            }

            if (output == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(output);
        }
    }
}
