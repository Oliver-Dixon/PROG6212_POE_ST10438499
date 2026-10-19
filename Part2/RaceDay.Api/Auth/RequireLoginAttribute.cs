using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Auth;

// Protects an endpoint using the server-side session created at login.
//   [RequireLogin]                 any logged-in user
//   [RequireLogin(Roles.Organiser)] Organisers only
// No session          -> 401 Unauthorized
// Wrong role          -> 403 Forbidden
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireLoginAttribute : Attribute, IAuthorizationFilter
{
    private readonly string? _role;

    public RequireLoginAttribute(string? role = null)
    {
        _role = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;

        if (session.GetInt32(SessionKeys.UserId) is null)
        {
            context.Result = new UnauthorizedObjectResult(new MessageResponse("You must log in to use this endpoint."));
            return;
        }

        if (_role != null && session.GetString(SessionKeys.Role) != _role)
        {
            context.Result = new ObjectResult(new MessageResponse($"Only users with the {_role} role can use this endpoint."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
