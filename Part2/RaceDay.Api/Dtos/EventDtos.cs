using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class EventRequest
{
    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>1 Road Running, 2 Trail Running, 3 Walking, 4 Cycling (see GET /api/event-types).</summary>
    [Range(1, int.MaxValue)]
    public int EventTypeId { get; set; }

    /// <summary>The location of the event (see GET /api/venues).</summary>
    [Range(1, int.MaxValue)]
    public int VenueId { get; set; }

    /// <summary>Format yyyy-MM-dd.</summary>
    [Required]
    public DateOnly? EventDate { get; set; }

    /// <summary>Format yyyy-MM-dd. Must be on or before the event date.</summary>
    [Required]
    public DateOnly? RegistrationDeadline { get; set; }
}

public class UpdateEventRequest : EventRequest
{
    /// <summary>Draft, Open, Closed, Completed or Cancelled. Participants can only enter Open events.</summary>
    [Required]
    public string Status { get; set; } = string.Empty;
}

public record EventTypeResponse(int EventTypeId, string TypeName, string? Description);

public record VenueResponse(int VenueId, string VenueName, string? AddressLine, string City, string Province, decimal Latitude, decimal Longitude);

public record EventResponse(
    int EventId,
    string EventName,
    string? Description,
    DateOnly EventDate,
    DateOnly RegistrationDeadline,
    string Status,
    string EventType,
    int OrganiserId,
    string OrganiserName,
    VenueResponse Location,
    List<decimal> DistancesKm,
    List<CategoryResponse> Categories);
