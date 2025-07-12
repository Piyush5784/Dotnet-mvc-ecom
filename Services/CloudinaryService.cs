
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using VMart.Interfaces;
using VMart.Models;

namespace VMart.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<(bool Success, string? Url, string? ErrorMessage)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, null, "No file uploaded");

            List<string> validExtensions = new() { ".jpg", ".png", ".jpeg" };
            string extension = Path.GetExtension(file.FileName).ToLower();

            if (!file.ContentType.StartsWith("image/"))
                return (false, null, "Only image files are allowed.");

            if (!validExtensions.Contains(extension))
                return (false, null, $"Invalid extension. Allowed: {string.Join(", ", validExtensions)}");

            if (file.Length > 5 * 1024 * 1024)
                return (false, null, "Max file size is 5MB");
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = Guid.NewGuid().ToString(),
                Folder = "vmart/products"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
                return (true, result.SecureUrl.ToString(), null);

            return (false, null, result.Error?.Message ?? "Unknown error");

        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
}
