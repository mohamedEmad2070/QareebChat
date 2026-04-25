using QareebChat.Abstractions;
using QareebChat.Contracts.Authentication;

namespace QareebChat.Services.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> GetTokenAsync(string emailOrUsername, string password, CancellationToken cancellationToken = default);

    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<Result> ConfirmEmailAsync(ConfirmationEmailRequest request);

    Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, CancellationToken cancellationToken = default);
}
