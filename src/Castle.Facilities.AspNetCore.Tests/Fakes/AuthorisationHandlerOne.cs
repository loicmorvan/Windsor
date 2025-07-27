namespace Castle.Facilities.AspNetCore.Tests.Fakes;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

public class AuthorisationHandlerOne : IAuthorizationHandler
{
	public Task HandleAsync(AuthorizationHandlerContext context)
	{
		return Task.CompletedTask;
	}
}