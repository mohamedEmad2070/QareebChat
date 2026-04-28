namespace QareebChat.Services.FileService;

public interface IFileService
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    void Delete(string? relativePath);
}
