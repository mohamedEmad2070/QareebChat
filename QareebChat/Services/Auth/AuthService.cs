using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using QareebChat.Abstractions;
using QareebChat.Abstractions.Consts;
using QareebChat.Authentication;
using QareebChat.Contracts.Authentication;
using QareebChat.Entities;
using QareebChat.Errors;
using QareebChat.Helper;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace QareebChat.Services.Auth;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IJwtProvider jwtProvider,
    SignInManager<ApplicationUser> signInManager,ILogger<AuthService> logger,
    IEmailSender emailService
    , IHttpContextAccessor accessor) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly int _refreshTokenExpiryInDays = 15;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IEmailSender _emailService = emailService;
    private readonly IHttpContextAccessor _accessor = accessor;

    public async Task<Result<AuthResponse>> GetTokenAsync(string emailOrUsername, string password, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(emailOrUsername);

        if (user is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

        if (!result.Succeeded)
        {
            var error = result.IsNotAllowed
                ? UserErrors.EmailNotConfirmed
                : result.IsLockedOut
                ? UserErrors.LockedUser
                : UserErrors.InvalidCredentials;

            return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
        }

        var (token, expiresIn) = _jwtProvider.GenerateToken(user);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryInDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresOn = refreshTokenExpiryDate
        });

        await _userManager.UpdateAsync(user);

        var response = new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, token, expiresIn, refreshToken, refreshTokenExpiryDate);

        return Result.Success(response);
    }

    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken,
       CancellationToken cancellationToken = default)
    {
        string? userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);
        }

        if (user.LockoutEnd > DateTime.UtcNow)
        {
            return Result.Failure<AuthResponse>(UserErrors.LockedUser);
        }

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && !x.IsRevoked);

        if (userRefreshToken is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);
        }

        if (userRefreshToken.IsExpired)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);
        }

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryInDays);

        user.RefreshTokens.Add(new RefreshToken()
        {
            Token = newRefreshToken,
            ExpiresOn = refreshTokenExpiryDate
        }
        );

        await _userManager.UpdateAsync(user);

        var response = new AuthResponse(user.Id, user.FirstName, user.LastName, user.Email, newToken, expiresIn, newRefreshToken, refreshTokenExpiryDate);

        return Result.Success(response);
    }
    private async Task<ApplicationUser?> FindUserAsync(string emailOrUsername)
    {
        if (emailOrUsername.Contains('@'))
        {
            var byEmail = await _userManager.FindByEmailAsync(emailOrUsername);
            if (byEmail is not null)
            {
                return byEmail;
            }
        }

        return await _userManager.FindByNameAsync(emailOrUsername);
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (emailIsExists)
            return Result.Failure<AuthResponse>(UserErrors.DuplicateEmail);

        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation("Email confirmation code: {Code}", code);

            await SendConfirmationEmail(user, code);
            return Result.Success();
        }

        var error = result.Errors.First();

        return Result.Failure<AuthResponse>(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken,
    CancellationToken cancellationToken = default)
    {
        string? userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidToken);
        }

        if (user.LockoutEnd > DateTime.UtcNow)
        {
            return Result.Failure<AuthResponse>(UserErrors.LockedUser);
        }

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken && !x.IsRevoked);

        if (userRefreshToken is null)
        {
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);
        }

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    private async Task SendConfirmationEmail(ApplicationUser user, string code)
    {
        var origin = _accessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBuilder.GenerateEmailBody("confirmation-email",
            new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName },
                { "{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}" }
            }
        );

        await _emailService.SendEmailAsync(user.Email!, "✅ Qareeb Chat: Email Confirmation", emailBody);
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmationEmailRequest request)
    {
        if (await _userManager.FindByIdAsync(request.Id) is not { } user)
            return Result.Failure(UserErrors.InvalidCode);

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.IsConfirmedBefore);

        var code = request.Code;

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, DefaultRoles.Member);
            return Result.Success();
        }

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.IsConfirmedBefore);

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Email confirmation code resent: {Code}", code);

        await SendConfirmationEmail(user, code);
        return Result.Success();
    }

    public async Task<Result> ForgetPasswordAsync(ForgetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return Result.Success();

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Password reset code: {Code}", code);

        await SendPasswordResetEmail(user, code);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.Id);

        if (user is null)
            return Result.Failure(UserErrors.InvalidCode);

        var code = request.Token;

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        var result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    private async Task SendPasswordResetEmail(ApplicationUser user, string code)
    {
        var origin = _accessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBuilder.GenerateEmailBody("reset-password",
            new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName },
                { "{{action_url}}", $"{origin}/auth/resetPassword?userId={user.Id}&code={code}" }
            }
        );

        await _emailService.SendEmailAsync(user.Email!, "🔐 Qareeb Chat: Password Reset", emailBody);
    }
}


