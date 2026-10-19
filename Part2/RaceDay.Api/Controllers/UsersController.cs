using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers;

[Route("api/users")]
public class UsersController : RaceDayControllerBase
{
    private readonly RaceDayContext _db;

    public UsersController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>Returns the logged-in user's profile.</summary>
    /// <response code="200">The profile of the user in the current session.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet("me")]
    [RequireLogin]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        if (user == null)
            return Unauthorized(new MessageResponse("Your account could not be found. Please log in again."));

        return Ok(user.ToResponse());
    }

    /// <summary>Updates the logged-in user's personal and emergency contact details.</summary>
    /// <remarks>Email, password and role cannot be changed here.</remarks>
    /// <response code="200">The profile was updated. Returns the updated profile.</response>
    /// <response code="400">A value is invalid, for example a date of birth in the future.</response>
    /// <response code="401">Not logged in.</response>
    [HttpPut("me")]
    [RequireLogin]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request)
    {
        if (request.DateOfBirth > Today)
            return BadRequest(new MessageResponse("Date of birth cannot be in the future."));

        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        if (user == null)
            return Unauthorized(new MessageResponse("Your account could not be found. Please log in again."));

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber;
        user.DateOfBirth = request.DateOfBirth;
        user.City = request.City;
        user.EmergencyContactName = request.EmergencyContactName;
        user.EmergencyContactPhone = request.EmergencyContactPhone;
        await _db.SaveChangesAsync();

        return Ok(user.ToResponse());
    }
}
