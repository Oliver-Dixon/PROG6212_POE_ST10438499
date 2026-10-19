using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public record UserProfileResponse(
    int UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? City,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    DateTime CreatedAt);

public class UpdateProfileRequest
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>Format yyyy-MM-dd. Used to check category minimum ages.</summary>
    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }

    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }
}
