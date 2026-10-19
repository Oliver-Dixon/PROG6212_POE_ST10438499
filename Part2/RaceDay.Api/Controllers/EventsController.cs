using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api/events")]
public class EventsController : RaceDayControllerBase
{
    private static readonly string[] Statuses = { "Draft", "Open", "Closed", "Completed", "Cancelled" };
    private readonly RaceDayContext _db;

    public EventsController(RaceDayContext db)
    {
        _db = db;
    }

    // Events with everything the response needs: type, organiser, venue, categories and their entries
    private IQueryable<Event> EventsWithDetails() => _db.Events
        .Include(e => e.EventType)
        .Include(e => e.Organiser)
        .Include(e => e.Venue)
        .Include(e => e.Categories).ThenInclude(c => c.Enrolments);

    /// <summary>Lists all published events so participants can browse upcoming races.</summary>
    /// <remarks>Draft events are hidden. Each event includes its name, description, date, location, distances and event type.</remarks>
    /// <response code="200">All events that are not in Draft status, soonest first.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents()
    {
        var events = await EventsWithDetails().Where(e => e.Status != "Draft").ToListAsync();
        return Ok(events.OrderBy(e => e.EventDate).Select(e => e.ToResponse()));
    }

    /// <summary>Returns one event with its venue and categories.</summary>
    /// <response code="200">The event.</response>
    /// <response code="404">No event has this ID.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvent(int id)
    {
        var ev = await EventsWithDetails().FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null)
            return NotFound(new MessageResponse($"Event {id} does not exist."));

        return Ok(ev.ToResponse());
    }

    /// <summary>Creates a new event for the logged-in organiser.</summary>
    /// <remarks>New events start in Draft status. Use PUT /api/events/{id} with status "Open" to publish it for entries.</remarks>
    /// <response code="201">The event was created. Returns the new event.</response>
    /// <response code="400">Invalid dates (in the past, or the deadline is after the event date), an unknown event type or venue, or an event with the same name on the same date already exists.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">The logged-in user is not an Organiser.</response>
    [HttpPost]
    [RequireLogin(Roles.Organiser)]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateEvent(EventRequest request)
    {
        if (request.EventDate < Today)
            return BadRequest(new MessageResponse("The event date cannot be in the past."));

        var error = await ValidateEventAsync(request, eventId: 0);
        if (error != null)
            return BadRequest(new MessageResponse(error));

        var ev = new Event
        {
            OrganiserId = CurrentUserId,
            EventName = request.EventName.Trim(),
            Description = request.Description,
            EventTypeId = request.EventTypeId,
            VenueId = request.VenueId,
            EventDate = request.EventDate!.Value,
            RegistrationDeadline = request.RegistrationDeadline!.Value,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow
        };
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        var created = await EventsWithDetails().FirstAsync(e => e.EventId == ev.EventId);
        return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, created.ToResponse());
    }

    /// <summary>Updates the organiser's own event, including its status.</summary>
    /// <remarks>Status can be Draft, Open, Closed, Completed or Cancelled.</remarks>
    /// <response code="200">The event was updated. Returns the updated event.</response>
    /// <response code="400">An invalid status, invalid dates, an unknown event type or venue, or a duplicate name on the same date.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another organiser.</response>
    /// <response code="404">No event has this ID.</response>
    [HttpPut("{id:int}")]
    [RequireLogin(Roles.Organiser)]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(int id, UpdateEventRequest request)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null)
            return NotFound(new MessageResponse($"Event {id} does not exist."));
        if (ev.OrganiserId != CurrentUserId)
            return Forbidden("You can only change your own events.");

        var status = Statuses.FirstOrDefault(s => s.Equals(request.Status.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status == null)
            return BadRequest(new MessageResponse("Status must be Draft, Open, Closed, Completed or Cancelled."));

        var error = await ValidateEventAsync(request, id);
        if (error != null)
            return BadRequest(new MessageResponse(error));

        ev.EventName = request.EventName.Trim();
        ev.Description = request.Description;
        ev.EventTypeId = request.EventTypeId;
        ev.VenueId = request.VenueId;
        ev.EventDate = request.EventDate!.Value;
        ev.RegistrationDeadline = request.RegistrationDeadline!.Value;
        ev.Status = status;
        ev.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var updated = await EventsWithDetails().FirstAsync(e => e.EventId == id);
        return Ok(updated.ToResponse());
    }

    /// <summary>Deletes the organiser's own event (and its categories) if nobody has entered it.</summary>
    /// <response code="204">The event was deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another organiser.</response>
    /// <response code="404">No event has this ID.</response>
    /// <response code="409">The event already has enrolments. Set its status to Cancelled instead.</response>
    [HttpDelete("{id:int}")]
    [RequireLogin(Roles.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _db.Events.Include(e => e.Categories).ThenInclude(c => c.Enrolments)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null)
            return NotFound(new MessageResponse($"Event {id} does not exist."));
        if (ev.OrganiserId != CurrentUserId)
            return Forbidden("You can only delete your own events.");
        if (ev.Categories.Any(c => c.Enrolments.Any()))
            return Conflict(new MessageResponse("This event already has enrolments. Set its status to Cancelled instead."));

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Checks the rules shared by create and update. Returns an error message, or null when valid.
    private async Task<string?> ValidateEventAsync(EventRequest request, int eventId)
    {
        if (request.RegistrationDeadline > request.EventDate)
            return "The registration deadline must be on or before the event date.";
        if (!await _db.EventTypes.AnyAsync(t => t.EventTypeId == request.EventTypeId))
            return $"Event type {request.EventTypeId} does not exist.";
        if (!await _db.Venues.AnyAsync(v => v.VenueId == request.VenueId))
            return $"Venue {request.VenueId} does not exist.";

        var name = request.EventName.Trim();
        if (await _db.Events.AnyAsync(e => e.EventName == name && e.EventDate == request.EventDate && e.EventId != eventId))
            return "An event with this name already exists on that date.";

        return null;
    }
}
