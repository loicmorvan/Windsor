using Microsoft.AspNetCore.Authorization;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class AuthorisationHandlerTwo : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        return Task.CompletedTask;
    }
}