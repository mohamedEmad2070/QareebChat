using QareebChat.Abstractions;

namespace QareebChat.Services.Auth;

public interface IAuthService
{
    Task<Result> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<Result<string>> LoginAsync(string email, string password);
    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<Result> LogoutAsync(string userId);
}
