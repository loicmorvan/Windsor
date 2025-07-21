namespace Castle.Windsor.Facilities.AspNetCore.Tests.Fakes;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

public class AuthorisationHandlerOne : IAuthorizationHandler
{
	public Task HandleAsync(AuthorizationHandlerContext context)
	{
		return Task.CompletedTask;
	}
}

public class AuthorisationHandlerTwo : IAuthorizationHandler
{
	public Task HandleAsync(AuthorizationHandlerContext context)
	{
		return Task.CompletedTask;
	}
}

public class AuthorisationHandlerThree : IAuthorizationHandler
{
	public Task HandleAsync(AuthorizationHandlerContext context)
	{
		return Task.CompletedTask;
	}
}