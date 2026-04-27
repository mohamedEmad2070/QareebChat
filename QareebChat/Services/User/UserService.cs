using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QareebChat.Abstractions;
using QareebChat.Contracts.User;
using QareebChat.Entities;
using QareebChat.Errors;

namespace QareebChat.Services.User;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<GetProfileResponse>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<GetProfileResponse>(UserErrors.UserNotFound);

        var response = user.Adapt<GetProfileResponse>();

        return Result.Success(response);
    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        // Check for duplicate username only if it changed
        if (!string.Equals(user.UserName, request.UserName, StringComparison.OrdinalIgnoreCase))
        {
            var usernameExists = await _userManager.Users
                .AnyAsync(x => x.UserName == request.UserName && x.Id != userId, cancellationToken);

            if (usernameExists)
                return Result.Failure(UserErrors.DuplicateUserName);
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UserName = request.UserName;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        
        var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);

        if (!isCurrentPasswordValid)
            return Result.Failure(UserErrors.InvalidCurrentPassword);

        
        if (request.CurrentPassword == request.NewPassword)
            return Result.Failure(UserErrors.SamePassword);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
        {
            // Revoke all refresh tokens — force re-login on all devices
            user.RefreshTokens.ForEach(t => t.RevokedOn = DateTime.UtcNow);
            await _userManager.UpdateAsync(user);
            return Result.Success();
        }

        var error = result.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
}
