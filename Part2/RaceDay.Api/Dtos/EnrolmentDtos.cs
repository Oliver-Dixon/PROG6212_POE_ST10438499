using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class EnrolmentRequest
{
    /// <summary>The category (distance) to enter. The event is found from the category.</summary>
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public record EnrolmentResponse(
    int EnrolmentId,
    int ParticipantId,
    string? RaceNumber,
    string Status,
    DateTime EnrolledAt,
    int EventId,
    string EventName,
    DateOnly EventDate,
    int CategoryId,
    string CategoryName,
    decimal DistanceKm);

public record EntrantResponse(
    int EnrolmentId,
    int ParticipantId,
    string ParticipantName,
    string Email,
    int CategoryId,
    string CategoryName,
    string? RaceNumber,
    string Status);
