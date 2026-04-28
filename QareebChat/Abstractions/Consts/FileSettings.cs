namespace QareebChat.Abstractions.Consts;

public static class FileSettings
{
    public const int MaxFileSizeInMB = 2;
    public const int MaxFileSizeInBytes = MaxFileSizeInMB * 1024 * 1024;
    public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    public const string AvatarsPath = "avatars";
}
