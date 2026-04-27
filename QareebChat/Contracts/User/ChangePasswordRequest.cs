namespace QareebChat.Contracts.User;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
