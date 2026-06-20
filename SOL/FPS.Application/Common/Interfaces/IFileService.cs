using Microsoft.AspNetCore.Http;

namespace FPS.Application.Common.Interfaces;

public interface IFileService
{
    bool ValidateFile(IFormFile file, string[] allowedExtensions, double maxSizeMb);
    Task<(string original, string thumbnail)> UploadWithThumbnailAsync(IFormFile file, string folder);
    Task DeleteAsync(string url);
}
