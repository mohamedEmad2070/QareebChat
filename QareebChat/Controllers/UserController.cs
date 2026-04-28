using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QareebChat.Abstractions;
using QareebChat.Contracts.User;
using QareebChat.Extensions;
using QareebChat.Services.User;

namespace QareebChat.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var result = await _userService.GetProfileAsync(userId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var result = await _userService.UpdateProfileAsync(userId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var result = await _userService.ChangePasswordAsync(userId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("profile-picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
            return Unauthorized();

        var result = await _userService.UploadProfilePictureAsync(userId, file, cancellationToken);

        return result.IsSuccess
            ? Ok(new { ProfilePictureUrl = result.Value })
            : result.ToProblem();
    }
}
