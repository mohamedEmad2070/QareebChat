using FluentValidation;
using QareebChat.Abstractions.Consts;

namespace QareebChat.Contracts.Authentication;

public class ResetPasswordValidator: AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required");

        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Matches(RegexPatterns.Password);
    }
    
    
}