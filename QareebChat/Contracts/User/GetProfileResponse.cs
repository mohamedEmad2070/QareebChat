namespace QareebChat.Contracts.User;

public record GetProfileResponse(
    string Id,
    string FirstName,
    string LastName,
    string? UserName,
    string? Email,
    string? ProfilePictureUrl
);
