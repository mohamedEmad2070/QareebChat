using QareebChat.Abstractions;
using QareebChat.Contracts.User;

namespace QareebChat.Services.User;

public interface IUserService
{
    Task<Result<GetProfileResponse>> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result<string>> UploadProfilePictureAsync(string userId, IFormFile file, CancellationToken cancellationToken = default);
}
