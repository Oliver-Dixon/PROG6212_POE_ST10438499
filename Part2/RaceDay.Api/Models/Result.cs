using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// The outcome of one enrolment (zero or one result per enrolment)
public class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }

    // Finished, DNF (did not finish), DNS (did not start) or DSQ (disqualified)
    [MaxLength(10)]
    public string FinishStatus { get; set; } = "Finished";

    [Column(TypeName = "time(0)")]
    public TimeOnly? FinishTime { get; set; }

    public int? CategoryPosition { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime RecordedAt { get; set; }

    public Enrolment Enrolment { get; set; } = null!;
}
