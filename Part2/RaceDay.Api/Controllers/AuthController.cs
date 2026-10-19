using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api/auth")]
public class AuthController : RaceDayControllerBase
{
    private readonly RaceDayContext _db;

    public AuthController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>Creates a new Organiser or Participant account.</summary>
    /// <remarks>Role must be "Organiser" or "Participant". The password is hashed with BCrypt before it is stored.</remarks>
    /// <response code="201">The account was created. Returns the new user's profile (never the password).</response>
    /// <response code="400">A field is missing or invalid, the password is shorter than 8 characters, or the role is not Organiser or Participant.</response>
    /// <response code="409">An account with this email already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var roleName = request.Role.Trim().ToLower() switch
        {
            "organiser" => Roles.Organiser,
            "participant" => Roles.Participant,
            _ => null
        };
        if (roleName == null)
            return BadRequest(new MessageResponse("Role must be either Organiser or Participant."));

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new MessageResponse("An account with this email already exists."));

        var role = await _db.Roles.FirstAsync(r => r.RoleName == roleName);
        var user = new User
        {
            RoleId = role.RoleId,
            Role = role,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created("/api/users/me", user.ToResponse());
    }

    /// <summary>Checks the email and password and starts a server-side session.</summary>
    /// <remarks>The session stores the user's ID and role. The browser (or Swagger) keeps the session cookie and sends it with every later request.</remarks>
    /// <response code="200">Logged in. Returns the user's ID, name and role.</response>
    /// <response code="400">The email or password is missing.</response>
    /// <response code="401">The email or password is wrong.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new MessageResponse("Incorrect email or password."));

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
        HttpContext.Session.SetString(SessionKeys.Role, user.Role.RoleName);

        return Ok(new LoginResponse(user.UserId, $"{user.FirstName} {user.LastName}", user.Role.RoleName,
            "Logged in. Your session stays active for 30 minutes after your last request."));
    }
}
