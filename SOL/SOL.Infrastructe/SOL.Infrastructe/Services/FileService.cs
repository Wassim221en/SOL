using SOL.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;

namespace Template.Infrastructe.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;
    private const int MaxFileSize = 15 * 1024 * 1024; // 15 MB
    public FileService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public bool ValidateFile(IFormFile file, string[] allowedExtensions, double maxFileSizeInMB)
    {
        if (file == null) return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension)) return false;

        var maxBytes = maxFileSizeInMB * 1024 * 1024;
        if (file.Length > maxBytes) return false;

        return true;
    }

    /*public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("ملف غير صالح");
        if (file.Length > MaxFileSize)
            throw new ArgumentException("حجم الملف كبير جداً. الحد الأقصى المسموح هو 10 ميغابايت");
        var uploadsFolder = Path.Combine(_env.WebRootPath, folder);
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
    }*/
    public async Task<string> UploadAsync(IFormFile file, string folder,int quality=50)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("ملف غير صالح");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("حجم الملف كبير جداً. الحد الأقصى المسموح هو 10 ميغابايت");

        var uploadsFolder = Path.Combine(_env.WebRootPath, folder);

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}.jpeg";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using var image = await Image.LoadAsync(file.OpenReadStream());
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(1280, 1280)
        }));

        await image.SaveAsync(filePath, new JpegEncoder
        {
            Quality = quality,
        });

        return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("ملف غير صالح");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("حجم الملف كبير جداً. الحد الأقصى المسموح هو 15 ميغابايت");

        var uploadsFolder = Path.Combine(_env.WebRootPath, folder);

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
    }
    /*public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("ملف غير صالح");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("حجم الملف كبير جداً. الحد الأقصى المسموح هو 10 ميغابايت");

        var uploadsFolder = Path.Combine(_env.WebRootPath, folder);
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}.avif";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // حفظ الملف مؤقتًا
        var tempInput = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(file.FileName));
        await using (var tempStream = new FileStream(tempInput, FileMode.Create))
        {
            await file.CopyToAsync(tempStream);
        }

        // مسار FFmpeg - تأكد أنه مثبت في الحاوية
        var ffmpegPath = "ffmpeg"; // أو المسار الكامل مثل /usr/bin/ffmpeg
        var args = $"-y -i \"{tempInput}\" -c:v libaom-av1 -crf 30 -b:v 0 -pix_fmt yuv420p \"{filePath}\"";
        var processStartInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        string stdOutput = await process.StandardOutput.ReadToEndAsync();
        string stdError = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        File.Delete(tempInput);

        if (process.ExitCode != 0)
        {
            throw new Exception($"FFmpeg failed: {stdError}");
        }

        return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
    }
    */
    public async Task<(string originalUrl, string thumbnailUrl)> UploadWithThumbnailAsync(IFormFile file, string folder, int width = 150, int height = 150)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("ملف غير صالح");
        if (file.Length > MaxFileSize)
            throw new ArgumentException("حجم الملف كبير جداً. الحد الأقصى المسموح هو 10 ميغابايت");

        var uploadsFolder = Path.Combine(_env.WebRootPath, folder);
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var baseFileName = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(file.FileName);

        // Upload original image
        var originalFileName = $"{baseFileName}{extension}";
        var originalFilePath = Path.Combine(uploadsFolder, originalFileName);

        using (var stream = new FileStream(originalFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Create thumbnail
        var thumbnailFileName = $"{baseFileName}_thumb.webp";
        var thumbnailFilePath = Path.Combine(uploadsFolder, thumbnailFileName);

        using var image = await Image.LoadAsync(file.OpenReadStream());
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(350, 350),
            Mode = ResizeMode.Crop,
            Sampler = KnownResamplers.Hermite
        }));

        await image.SaveAsync(thumbnailFilePath, new WebpEncoder
        {
            Quality = 100,
            Method = WebpEncodingMethod.BestQuality
        });

        var originalUrl = Path.Combine(folder, originalFileName).Replace("\\", "/");
        var thumbnailUrl = Path.Combine(folder, thumbnailFileName).Replace("\\", "/");

        return (originalUrl, thumbnailUrl);
    }
    public async Task<string> CreateThumbnailAsync(IFormFile file, string folder, int width = 150, int height = 150)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file");

        var uploadsFolder = Path.Combine(_env.WebRootPath, folder);

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}_thumb.webp";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using var image = await Image.LoadAsync(file.OpenReadStream());

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Crop
        }));

        await image.SaveAsync(filePath, new WebpEncoder
        {
            Quality = 75
        });

        return Path.Combine(folder, fileName).Replace("\\", "/");
    }

    public Task DeleteAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return Task.CompletedTask;

        var fullPath = Path.Combine(_env.WebRootPath, filePath);
        if (!File.Exists(fullPath)) return Task.CompletedTask;

        File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
