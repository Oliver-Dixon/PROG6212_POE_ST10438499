using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Auth;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers;

// Shared helpers for every RaceDay controller
[ApiController]
[Produces("application/json")]
public abstract class RaceDayControllerBase : ControllerBase
{
    // The logged-in user's ID, read from the server-side session
    protected int CurrentUserId => HttpContext.Session.GetInt32(SessionKeys.UserId) ?? 0;

    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    protected ObjectResult Forbidden(string message) =>
        StatusCode(StatusCodes.Status403Forbidden, new MessageResponse(message));
}
