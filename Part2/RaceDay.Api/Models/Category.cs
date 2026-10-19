using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// A distance or age category offered in one event, e.g. "21.1 km Half Marathon"
public class Category
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }

    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(6,2)")]
    public decimal DistanceKm { get; set; }

    [Column(TypeName = "time(0)")]
    public TimeOnly StartTime { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal EntryFee { get; set; }

    public int MaxParticipants { get; set; }
    public byte MinimumAge { get; set; }

    public Event Event { get; set; } = null!;
    public List<Enrolment> Enrolments { get; set; } = new();
}
