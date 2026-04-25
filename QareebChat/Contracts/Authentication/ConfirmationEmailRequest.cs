namespace QareebChat.Contracts.Authentication;

public record ConfirmationEmailRequest
(
        string Id,
        string Code
);