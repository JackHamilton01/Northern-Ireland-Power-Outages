using Microsoft.AspNetCore.Components.Forms;

namespace NorthernIrelandPowerOutages.Models
{
    public class ImageWithPreviewUI
    {
        public IBrowserFile File { get; set; }
        public string PreviewUrl { get; set; }
    }
}
