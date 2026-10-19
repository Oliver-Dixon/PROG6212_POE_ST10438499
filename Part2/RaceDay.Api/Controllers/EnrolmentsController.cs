using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api")]
public class EnrolmentsController : RaceDayControllerBase
{
    private readonly RaceDayContext _db;

    public EnrolmentsController(RaceDayContext db)
    {
        _db = db;
    }

    // Enrolments with their category and event, for the responses
    private IQueryable<Enrolment> EnrolmentsWithDetails() => _db.Enrolments
        .Include(e => e.Participant)
        .Include(e => e.Category).ThenInclude(c => c.Event);

    /// <summary>Enters the logged-in participant into a category of an open event.</summary>
    /// <remarks>The enrolment links the participant, the selected category and (through the category) the event. A race number is assigned automatically.</remarks>
    /// <response code="201">The participant was entered. Returns the enrolment with its race number.</response>
    /// <response code="400">The event is not open, registration has closed, or the participant is younger than the category's minimum age.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">The logged-in user is not a Participant.</response>
    /// <response code="404">No category has this ID.</response>
    /// <response code="409">The participant has already entered this category, or the category is full.</response>
    [HttpPost("enrolments")]
    [RequireLogin(Roles.Participant)]
    [ProducesResponseType(typeof(EnrolmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(EnrolmentRequest request)
    {
        var category = await _db.Categories.Include(c => c.Event).Include(c => c.Enrolments)
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);
        if (category == null)
            return NotFound(new MessageResponse($"Category {request.CategoryId} does not exist."));

        if (category.Event.Status != "Open")
            return BadRequest(new MessageResponse("This event is not open for entries."));
        if (Today > category.Event.RegistrationDeadline)
            return BadRequest(new MessageResponse($"Registration for this event closed on {category.Event.RegistrationDeadline:yyyy-MM-dd}."));

        if (category.MinimumAge > 0)
        {
            var participant = await _db.Users.FindAsync(CurrentUserId);
            if (participant?.DateOfBirth == null)
                return BadRequest(new MessageResponse($"This category has a minimum age of {category.MinimumAge}. Add your date of birth to your profile first."));
            if (AgeOn(participant.DateOfBirth.Value, category.Event.EventDate) < category.MinimumAge)
                return BadRequest(new MessageResponse($"You must be at least {category.MinimumAge} on the event day to enter this category."));
        }

        if (category.Enrolments.Any(e => e.ParticipantId == CurrentUserId))
            return Conflict(new MessageResponse("You have already entered this category."));
        if (category.Enrolments.Count(e => e.Status != "Cancelled") >= category.MaxParticipants)
            return Conflict(new MessageResponse("This category is full."));

        var enrolment = new Enrolment
        {
            ParticipantId = CurrentUserId,
            CategoryId = category.CategoryId,
            Status = "Confirmed",
            EnrolledAt = DateTime.UtcNow
        };
        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        // The race number uses the new enrolment ID, so it is always unique
        enrolment.RaceNumber = $"RD{enrolment.EnrolmentId:D6}";
        await _db.SaveChangesAsync();

        var created = await EnrolmentsWithDetails().FirstAsync(e => e.EnrolmentId == enrolment.EnrolmentId);
        return Created("/api/users/me/enrolments", created.ToResponse());
    }

    /// <summary>Lists the logged-in participant's enrolments.</summary>
    /// <response code="200">The participant's enrolments with event, category and status, newest event first.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">The logged-in user is not a Participant.</response>
    [HttpGet("users/me/enrolments")]
    [RequireLogin(Roles.Participant)]
    [ProducesResponseType(typeof(List<EnrolmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var enrolments = await EnrolmentsWithDetails().Where(e => e.ParticipantId == CurrentUserId).ToListAsync();
        return Ok(enrolments.OrderByDescending(e => e.Category.Event.EventDate).Select(e => e.ToResponse()));
    }

    /// <summary>Cancels the participant's own enrolment.</summary>
    /// <remarks>The enrolment is not deleted. Its status is set to Cancelled so the history is kept.</remarks>
    /// <response code="204">The enrolment was cancelled.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not a Participant, or the enrolment belongs to someone else.</response>
    /// <response code="404">No enrolment has this ID.</response>
    [HttpDelete("enrolments/{id:int}")]
    [RequireLogin(Roles.Participant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var enrolment = await _db.Enrolments.FindAsync(id);
        if (enrolment == null)
            return NotFound(new MessageResponse($"Enrolment {id} does not exist."));
        if (enrolment.ParticipantId != CurrentUserId)
            return Forbidden("You can only cancel your own enrolments.");

        enrolment.Status = "Cancelled";
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Lists everyone entered in the organiser's own event.</summary>
    /// <response code="200">Every enrolment in the event with participant, category, race number and status.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another organiser.</response>
    /// <response code="404">No event has this ID.</response>
    [HttpGet("events/{eventId:int}/enrolments")]
    [RequireLogin(Roles.Organiser)]
    [ProducesResponseType(typeof(List<EntrantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null)
            return NotFound(new MessageResponse($"Event {eventId} does not exist."));
        if (ev.OrganiserId != CurrentUserId)
            return Forbidden("You can only view entries for your own events.");

        var enrolments = await EnrolmentsWithDetails().Where(e => e.Category.EventId == eventId).ToListAsync();
        return Ok(enrolments.OrderBy(e => e.CategoryId).ThenBy(e => e.EnrolmentId).Select(e => e.ToEntrant()));
    }

    private static int AgeOn(DateOnly dateOfBirth, DateOnly onDate)
    {
        var age = onDate.Year - dateOfBirth.Year;
        if (onDate < dateOfBirth.AddYears(age))
            age--;
        return age;
    }
}
