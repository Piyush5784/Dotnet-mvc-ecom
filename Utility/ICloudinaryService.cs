

namespace VMart.Services
{
    public interface ICloudinaryService
    {
        Task<(bool Success, string Url, string ErrorMessage)> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
