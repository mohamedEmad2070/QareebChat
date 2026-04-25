using System.ComponentModel.DataAnnotations;

namespace QareebChat.Contracts.Authentication;

public record ResetPasswordRequest(

    string Token,
    string Id,
    string NewPassword
);
