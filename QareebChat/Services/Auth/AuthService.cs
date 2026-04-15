using Microsoft.AspNetCore.Identity;
using QareebChat.Abstractions;
using QareebChat.Entities;

namespace QareebChat.Services.Auth;

public class AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : IAuthService
{
    public async Task<Result> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.FirstOrDefault();
        return Result.Failure(new Error(
            Code: error?.Code ?? "RegistrationFailed",
            Description: error?.Description ?? "User registration failed",
            StatusCode: StatusCodes.Status400BadRequest));
    }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return Result.Failure<string>(new Error(
                Code: "UserNotFound",
                Description: "User not found",
                StatusCode: StatusCodes.Status401Unauthorized));

        var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded)
            return Result.Success<string>(user.Id);

        var (code, description, statusCode) = result switch
        {
            _ when result.IsLockedOut => (
                "AccountLockedOut",
                "Account is locked out",
                StatusCodes.Status403Forbidden),
            _ when result.RequiresTwoFactor => (
                "TwoFactorRequired",
                "Two-factor authentication is required",
                StatusCodes.Status403Forbidden),
            _ => (
                "InvalidCredentials",
                "Invalid email or password",
                StatusCodes.Status401Unauthorized)
        };

        return Result.Failure<string>(new Error(
            Code: code,
            Description: description,
            StatusCode: statusCode));
    }

    public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(new Error(
                Code: "UserNotFound",
                Description: "User not found",
                StatusCode: StatusCodes.Status404NotFound));

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.FirstOrDefault();
        return Result.Failure(new Error(
            Code: error?.Code ?? "ChangePasswordFailed",
            Description: error?.Description ?? "Password change failed",
            StatusCode: StatusCodes.Status400BadRequest));
    }

    public async Task<Result> LogoutAsync(string userId)
    {
        try
        {
            await signInManager.SignOutAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(
                Code: "LogoutFailed",
                Description: ex.Message,
                StatusCode: StatusCodes.Status500InternalServerError));
        }
    }
}
