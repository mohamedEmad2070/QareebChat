namespace QareebChat.Services.FileService;

public class FileService(IWebHostEnvironment environment) : IFileService
{
    private readonly IWebHostEnvironment _environment = environment;

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        var folderPath = Path.Combine(_environment.WebRootPath, folder);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.CreateVersion7()}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        // Return relative URL e.g. /avatars/abc.jpg
        return $"/{folder}/{fileName}";
    }

    public void Delete(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return;

        // Strip leading slash and combine with wwwroot
        var fullPath = Path.Combine(
            _environment.WebRootPath,
            relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
        );

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
