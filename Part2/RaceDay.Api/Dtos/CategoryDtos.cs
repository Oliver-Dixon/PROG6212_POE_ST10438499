using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class CategoryRequest
{
    /// <summary>For example "10 km Road Race", "Under 20" or "Senior".</summary>
    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Range(0.01, 1000)]
    public decimal DistanceKm { get; set; }

    /// <summary>Format HH:mm:ss, for example 06:30:00.</summary>
    [Required]
    public TimeOnly? StartTime { get; set; }

    [Range(0, 100000)]
    public decimal EntryFee { get; set; }

    [Range(1, 100000)]
    public int MaxParticipants { get; set; }

    [Range(0, 100)]
    public byte MinimumAge { get; set; }
}

public record CategoryResponse(
    int CategoryId,
    int EventId,
    string CategoryName,
    decimal DistanceKm,
    TimeOnly StartTime,
    decimal EntryFee,
    int MaxParticipants,
    byte MinimumAge,
    int PlacesRemaining);
