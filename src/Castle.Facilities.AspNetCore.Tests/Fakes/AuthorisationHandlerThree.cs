using Microsoft.AspNetCore.Authorization;

namespace Castle.Facilities.AspNetCore.Tests.Fakes;

public class AuthorisationHandlerThree : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        return Task.CompletedTask;
    }
}