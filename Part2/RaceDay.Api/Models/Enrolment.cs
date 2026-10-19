using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// A Participant's entry into a Category (the junction table for the many-to-many relationship)
public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public int CategoryId { get; set; }

    [MaxLength(10)]
    public string? RaceNumber { get; set; }

    // Pending, Confirmed or Cancelled
    [MaxLength(20)]
    public string Status { get; set; } = "Confirmed";

    [Column(TypeName = "datetime2(0)")]
    public DateTime EnrolledAt { get; set; }

    public User Participant { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Result? Result { get; set; }
}
