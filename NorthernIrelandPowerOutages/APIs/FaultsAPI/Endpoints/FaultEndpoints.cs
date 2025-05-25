using FaultsAPI.Data;
using FaultsAPI.Models;

namespace FaultsAPI.Endpoints
{
    public static class FaultEndpoints
    {
        public static void AddFaultEndpoints(this WebApplication app)
        {
            app.MapGet("/faults", LoadAllFaultsAsync);
            app.MapGet("/faults/{incidentReference}", LoadFaultByIncidentReferenceAsync);
        }

        private static async Task<IResult> LoadAllFaultsAsync(FaultData data, string? outageType, string? search, int? delay)
        {
            FaultModel? faults = await data.LoadFaultsAsync();

            if (outageType != null)
            {
                if (!Enum.TryParse<OutageType>(outageType, true, out var parsedPowerCutType))
                {
                    return Results.BadRequest($"Invalid power cut type: {outageType}");
                }

                faults.OutageMessage = faults.OutageMessage
                    .Where(x => x.OutageType == outageType)
                    .ToArray();
            }

            if (string.IsNullOrWhiteSpace(search) == false)
            {
                faults.OutageMessage = faults.OutageMessage
                    .Where(x => x.PostCode != null && x.PostCode.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
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

            return Results.Ok(faults);
        }

        private static async Task<IResult> LoadAllFaultsAsyncFromJson(FaultData data, string? outageType, string? search, int? delay)
        {
            var faults = await data.LoadFaultsAsync();

            if (outageType != null)
            {
                if (!Enum.TryParse<OutageType>(outageType, true, out var parsedPowerCutType))
                {
                    return Results.BadRequest($"Invalid power cut type: {outageType}");
                }

                faults.OutageMessage = faults.OutageMessage
                    .Where(x => x.OutageType == outageType)
                    .ToArray();
            }

            if (string.IsNullOrWhiteSpace(search) == false)
            {
                faults.OutageMessage = faults.OutageMessage
                    .Where(x => x.PostCode != null && x.PostCode.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
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

            return Results.Ok(faults);
        }

        private static async Task<IResult> LoadFaultByIncidentReferenceAsync(FaultData data, string outageId, int? delay)
        {
            var faults = await data.LoadFaultsAsync();

            if (delay is not null)
            {
                // Max delay of 5 minutes (300,000 milliseconds)
                if (delay > 300000)
                {
                    delay = 300000;
                }

                await Task.Delay((int)delay);
            }

            var match = faults.OutageMessage.SingleOrDefault(x => x.OutageId == outageId);
            if (match != null)
            {
                return Results.Ok(match);
            }

            return Results.NotFound();
        }
    }
}
