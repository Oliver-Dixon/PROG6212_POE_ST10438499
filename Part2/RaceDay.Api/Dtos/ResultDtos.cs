using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class UpdateResultRequest
{
    /// <summary>Finished, DNF (did not finish), DNS (did not start) or DSQ (disqualified).</summary>
    [Required]
    public string FinishStatus { get; set; } = string.Empty;

    /// <summary>Format HH:mm:ss. Required when Finished, empty otherwise.</summary>
    public TimeOnly? FinishTime { get; set; }

    /// <summary>Position in the category. Required when Finished, empty otherwise.</summary>
    [Range(1, int.MaxValue)]
    public int? CategoryPosition { get; set; }
}

public class ResultRequest : UpdateResultRequest
{
    [Range(1, int.MaxValue)]
    public int EnrolmentId { get; set; }
}

public record ResultResponse(
    int ResultId,
    int EnrolmentId,
    int ParticipantId,
    string ParticipantName,
    int EventId,
    string EventName,
    DateOnly EventDate,
    string CategoryName,
    decimal DistanceKm,
    string FinishStatus,
    TimeOnly? FinishTime,
    int? CategoryPosition);
