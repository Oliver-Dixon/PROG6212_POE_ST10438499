using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

// One of the two system roles: Organiser or Participant
public class Role
{
    public int RoleId { get; set; }

    [MaxLength(20)]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public List<User> Users { get; set; } = new();
}
