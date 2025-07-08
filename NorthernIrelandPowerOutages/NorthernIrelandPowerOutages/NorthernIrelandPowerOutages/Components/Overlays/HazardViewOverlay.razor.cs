using Domain.Frontend;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis.CSharp;
using NorthernIrelandPowerOutages.Models;
using Twilio.TwiML.Voice;

namespace NorthernIrelandPowerOutages.Components.Overlays
{
    public partial class HazardViewOverlay
    {
        [Parameter]
        public EventCallback OnClose { get; set; }

        [Parameter]
        public HazardUI Hazard { get; set; }

        private void CloseOverlay(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            OnClose.InvokeAsync();
        }

        private string GetImagePreviewUrl(string fileName)
        {
            string? fileStorageLocation = config.GetValue<string>("WebStorageRoot");
            if (string.IsNullOrWhiteSpace(fileStorageLocation))
            {
                throw new Exception("File storage location is not configured.");
            }

            string path = Path.Combine(
                fileStorageLocation,
                fileName);

            return path;
        }
    }
}