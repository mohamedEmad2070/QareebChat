using Microsoft.AspNetCore.Http;
using QareebChat.Abstractions;
using QareebChat.Abstractions.Consts;

namespace QareebChat.Errors;

public static class UserErrors
{
    public static readonly Error InvalidCredentials = new("Users.InvalidCredentials", "Invalid email/username or password.", StatusCodes.Status400BadRequest);
    public static readonly Error EmailNotConfirmed = new("Users.EmailNotConfirmed", "Email is not confirmed.", StatusCodes.Status400BadRequest);
    public static readonly Error LockedUser = new("Users.LockedUser", "User account is locked.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidToken = new("Users.InvalidToken", "Invalid token.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidRefreshToken = new("Users.InvalidRefreshToken", "Invalid refresh token.", StatusCodes.Status400BadRequest);
    public static readonly Error DuplicateEmail = new("Users.DuplicateEmail", "Email is already registered.", StatusCodes.Status400BadRequest);
    public static readonly Error DuplicateUserName = new("Users.DuplicateUserName", "Username is already taken.", StatusCodes.Status400BadRequest);
    public static readonly Error DuplicateEmailConfirmed = new("Users.DuplicateEmailConfirmed", "Email is already confirmed.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidCode = new("Users.InvalidCode", "Invalid confirmation code.", StatusCodes.Status400BadRequest);
    public static readonly Error IsConfirmedBefore = new("Users.IsConfirmedBefore", "Email is already confirmed.", StatusCodes.Status400BadRequest);
    public static readonly Error UserNotFound = new("Users.UserNotFound", "User not found.", StatusCodes.Status404NotFound);
    public static readonly Error InvalidCurrentPassword = new("Users.InvalidCurrentPassword", "Current password is incorrect.", StatusCodes.Status400BadRequest);
    public static readonly Error SamePassword = new("Users.SamePassword", "New password must be different from current password.", StatusCodes.Status400BadRequest);
    public static readonly Error InvalidImageExtension = new("Users.InvalidImageExtension", $"Only {string.Join(", ", FileSettings.AllowedExtensions)} files are allowed.", StatusCodes.Status400BadRequest);
    public static readonly Error FileSizeExceeded = new("Users.FileSizeExceeded", $"File size must not exceed {FileSettings.MaxFileSizeInMB}MB.", StatusCodes.Status400BadRequest);
    public static readonly Error NoFileUploaded = new("Users.NoFileUploaded", "Please upload a file.", StatusCodes.Status400BadRequest);
}