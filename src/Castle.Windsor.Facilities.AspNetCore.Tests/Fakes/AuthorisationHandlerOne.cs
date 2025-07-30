using Microsoft.AspNetCore.Authorization;

namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

public class AuthorisationHandlerOne : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        return Task.CompletedTask;
    }
}