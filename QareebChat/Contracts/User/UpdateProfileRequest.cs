namespace QareebChat.Contracts.User;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string UserName
);
