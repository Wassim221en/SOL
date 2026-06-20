using Microsoft.AspNetCore.Http;

namespace SOL.Application.Common.Interfaces;

public interface IFileService
{
    bool ValidateFile(IFormFile file, string[] allowedExtensions, double maxFileSizeInMB);
    Task<string> UploadAsync(IFormFile file, string folder, int quality = 50);
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task<(string originalUrl, string thumbnailUrl)> UploadWithThumbnailAsync(IFormFile file, string folder, int width = 150, int height = 150);
    Task<string> CreateThumbnailAsync(IFormFile file, string folder, int width = 150, int height = 150);
    Task DeleteAsync(string filePath);
}
