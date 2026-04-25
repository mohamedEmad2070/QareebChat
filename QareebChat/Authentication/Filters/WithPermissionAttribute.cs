using Microsoft.AspNetCore.Authorization;

namespace QareebChat.Authentication.Filters;

public class WithPermissionAttribute(string permission) : AuthorizeAttribute(permission)
{
}