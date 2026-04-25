namespace QareebChat.Contracts.Authentication;

public record LoginRequest(
    string EmailOrUsername,
    string Password
);