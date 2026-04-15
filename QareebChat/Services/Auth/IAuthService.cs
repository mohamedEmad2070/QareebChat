namespace QareebChat.Services.Auth;

public interface IAuthService
{
    Task<bool> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<string?> LoginAsync(string email, string password);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> LogoutAsync(string userId);
}
