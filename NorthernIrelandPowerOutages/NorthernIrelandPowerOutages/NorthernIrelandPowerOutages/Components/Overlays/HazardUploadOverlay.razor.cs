using Domain.Frontend;
using HazardVerifyService;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using NorthernIrelandPowerOutages.Models;
using System;

namespace NorthernIrelandPowerOutages.Components.Overlays
{
    public partial class HazardUploadOverlay
    {
        private bool isSaving = false;
        private string response = "No response yet";
        private HazardUI hazardInput = new();
        private List<ImageWithPreviewUI> uploadedImages = new();

        private long maxFileSize = 3 * 1024 * 1024; // 3 MB
        private int maxAllowedFiles = 3;
        private List<string> errors = new();
        private IBrowserFile? file = null;
        private bool isMaxNumberOfImages => uploadedImages.Count != 3;

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public EventCallback OnClose { get; set; }

        [Parameter]
        public double Latitude { get; set; }

        [Parameter]
        public double Longitude { get; set; }

        private void CloseOverlay() => OnClose.InvokeAsync();


        private string CreateWebPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            return Path.Combine(config.GetValue<string>("WebStorageRoot"), relativePath);
        }

        private async Task<string> GetImagePreviewUrl(IBrowserFile file)
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 10_000_000);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var base64 = Convert.ToBase64String(ms.ToArray());
            return $"data:{file.ContentType};base64,{base64}";
        }

        private async Task SaveHazard(EditContext args)
        {
            try
            {
                hazardInput.Images = new();
                foreach (var image in uploadedImages)
                {
                    hazardInput.Images.Add(new()
                    {
                        FileName = image.RelativePath,
                    });

                    isSaving = true;
                    StateHasChanged();

                    var result = await LlavaClient.CallLlavaAsync(Path.Combine(config.GetValue<string>("FileStorage"), image.RelativePath),
                        $"Is this an image of {hazardInput.Title}. Start your reply with either Yes or No," +
                        $"then separating with a semi-colon(;)" +
                        $"describe the image provided");


                    var llavaResponse = result.Response.Split(';');
                    response = llavaResponse[1];

                    if (llavaResponse[0].StartsWith("No"))
                    {
                        isSaving = false;
                        errors.Add(response);
                        response = string.Empty;
                        return;
                    }
                }
                hazardInput.Latitude = Latitude;
                hazardInput.Longitude = Longitude;

                DbContext.Hazards.Add(hazardInput);
                DbContext.SaveChanges();
                isSaving = false;
            }
            catch (Exception ex)
            {
                errors.Add($"Error: {ex.Message}");
                isSaving = false;
            }
        }

        private void RemoveImage(ImageWithPreviewUI image)
        {
            uploadedImages.Remove(image);
        }

        private async Task UploadHazards(InputFileChangeEventArgs e)
        {
            file = e.File;

            var previewUrl = await GetImagePreviewUrl(file);

            ImageWithPreviewUI? imageWithPreview = new ImageWithPreviewUI
            {
                File = e.File,
                PreviewUrl = previewUrl,
            };

            uploadedImages.Add(imageWithPreview);
            imageWithPreview.RelativePath = await CaptureFile(imageWithPreview);
        }

        private async Task<string> CaptureFile(ImageWithPreviewUI image)
        {
            if (file is null)
            {
                return string.Empty;
            }

            try
            {
                string newFileName = Path.ChangeExtension(
                    Path.GetRandomFileName(),
                    Path.GetExtension(image.File.Name)); // Trust extension from upload but not file name

                string? fileStorageLocation = config.GetValue<string>("FileStorage");
                if (string.IsNullOrWhiteSpace(fileStorageLocation))
                {
                    throw new Exception("File storage location is not configured.");
                }

                string path = Path.Combine(
                    fileStorageLocation,
                    "jhamilton",
                    newFileName);

                string relativePath = Path.Combine(
                    "jhamilton",
                    newFileName);

                Directory.CreateDirectory(Path.Combine(
                    fileStorageLocation,
                    "jhamilton"));

                await using FileStream fs = new(path, FileMode.Create);
                await image.File.OpenReadStream(maxFileSize).CopyToAsync(fs);
                return relativePath;
            }
            catch (Exception)
            {
                errors.Add($"Error: unable to upload file {file.Name}");
                throw;
            }

            return null;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }
}