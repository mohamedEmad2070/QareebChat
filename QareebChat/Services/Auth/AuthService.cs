using Microsoft.AspNetCore.Identity;
using QareebChat.Abstractions;
using QareebChat.Entities;

namespace QareebChat.Services.Auth;

public class AuthService(UserManager<ApplicationUser> userManager) : IAuthService
{
    private readonly UserManager<ApplicationUser> _UserManager = userManager;
}
