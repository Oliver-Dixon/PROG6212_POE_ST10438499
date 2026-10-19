using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// A race day created by an Organiser
public class Event
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public int EventTypeId { get; set; }
    public int VenueId { get; set; }

    [MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateOnly EventDate { get; set; }
    public DateOnly RegistrationDeadline { get; set; }

    // Draft, Open, Closed, Completed or Cancelled
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? RouteMapUrl { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? UpdatedAt { get; set; }

    public User Organiser { get; set; } = null!;
    public EventType EventType { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public List<Category> Categories { get; set; } = new();
}
