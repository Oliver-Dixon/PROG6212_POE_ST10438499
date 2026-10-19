using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class RegisterRequest
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>At least 8 characters. It is hashed with BCrypt before it is stored.</summary>
    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Organiser or Participant.</summary>
    [Required]
    public string Role { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Returned after a successful login. The session cookie keeps the user logged in.</summary>
public record LoginResponse(int UserId, string FullName, string Role, string Message);
