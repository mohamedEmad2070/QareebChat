namespace QareebChat.Contracts.Authentication;

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);