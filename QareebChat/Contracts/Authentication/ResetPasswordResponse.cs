namespace QareebChat.Contracts.Authentication;

public record ResetPasswordResponse
{
    public required string Message { get; init; }
}
