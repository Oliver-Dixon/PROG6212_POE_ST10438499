using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// The location of an event. The coordinates are used for live weather in Part 3.
public class Venue
{
    public int VenueId { get; set; }

    [MaxLength(150)]
    public string VenueName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AddressLine { get; set; }

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Province { get; set; } = string.Empty;

    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

    public List<Event> Events { get; set; } = new();
}
