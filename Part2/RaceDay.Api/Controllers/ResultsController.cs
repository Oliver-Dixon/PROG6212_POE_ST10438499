using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api")]
public class ResultsController : RaceDayControllerBase
{
    private static readonly string[] FinishStatuses = { "Finished", "DNF", "DNS", "DSQ" };
    private readonly RaceDayContext _db;

    public ResultsController(RaceDayContext db)
    {
        _db = db;
    }

    // Results with the participant, category and event, for the responses
    private IQueryable<Result> ResultsWithDetails() => _db.Results
        .Include(r => r.Enrolment).ThenInclude(e => e.Participant)
        .Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event);

    /// <summary>Records a participant's result for the organiser's own event.</summary>
    /// <remarks>A finisher needs a finish time (HH:mm:ss) and a position. DNF, DNS and DSQ results have neither.</remarks>
    /// <response code="201">The result was recorded. Returns the result.</response>
    /// <response code="400">The finish time or position is missing for a finisher or given for a DNF/DNS/DSQ, the entry was cancelled, or the event has not happened yet.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another organiser.</response>
    /// <response code="404">No enrolment has this ID.</response>
    /// <response code="409">A result has already been recorded for this enrolment. Use PUT /api/results/{id} to correct it.</response>
    [HttpPost("results")]
    [RequireLogin(Roles.Organiser)]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RecordResult(ResultRequest request)
    {
        var enrolment = await _db.Enrolments.Include(e => e.Category).ThenInclude(c => c.Event)
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId);
        if (enrolment == null)
            return NotFound(new MessageResponse($"Enrolment {request.EnrolmentId} does not exist."));
        if (enrolment.Category.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only record results for your own events.");

        if (enrolment.Status == "Cancelled")
            return BadRequest(new MessageResponse("This entry was cancelled, so it cannot have a result."));
        if (enrolment.Category.Event.EventDate > Today)
            return BadRequest(new MessageResponse("Results can only be recorded once the event has taken place."));

        var error = ValidateResult(request, out var finishStatus);
        if (error != null)
            return BadRequest(new MessageResponse(error));

        if (enrolment.Result != null)
            return Conflict(new MessageResponse($"A result is already recorded for this enrolment. Use PUT /api/results/{enrolment.Result.ResultId} to correct it."));

        var result = new Result
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishStatus = finishStatus,
            FinishTime = request.FinishTime,
            CategoryPosition = request.CategoryPosition,
            RecordedAt = DateTime.UtcNow
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        var created = await ResultsWithDetails().FirstAsync(r => r.ResultId == result.ResultId);
        return Created($"/api/events/{enrolment.Category.EventId}/results", created.ToResponse());
    }

    /// <summary>Corrects a recorded result.</summary>
    /// <response code="200">The result was updated. Returns the updated result.</response>
    /// <response code="400">The finish time or position does not match the finish status.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another organiser.</response>
    /// <response code="404">No result has this ID.</response>
    [HttpPut("results/{id:int}")]
    [RequireLogin(Roles.Organiser)]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateResult(int id, UpdateResultRequest request)
    {
        var result = await ResultsWithDetails().FirstOrDefaultAsync(r => r.ResultId == id);
        if (result == null)
            return NotFound(new MessageResponse($"Result {id} does not exist."));
        if (result.Enrolment.Category.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only change results for your own events.");

        var error = ValidateResult(request, out var finishStatus);
        if (error != null)
            return BadRequest(new MessageResponse(error));

        result.FinishStatus = finishStatus;
        result.FinishTime = request.FinishTime;
        result.CategoryPosition = request.CategoryPosition;
        result.RecordedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(result.ToResponse());
    }

    /// <summary>Lists an event's results in finishing order.</summary>
    /// <response code="200">The event's results, grouped by category and ordered by position (DNF, DNS and DSQ last).</response>
    /// <response code="404">No event has this ID.</response>
    [HttpGet("events/{eventId:int}/results")]
    [ProducesResponseType(typeof(List<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
            return NotFound(new MessageResponse($"Event {eventId} does not exist."));

        var results = await ResultsWithDetails().Where(r => r.Enrolment.Category.EventId == eventId).ToListAsync();
        return Ok(results
            .OrderBy(r => r.Enrolment.CategoryId)
            .ThenBy(r => r.CategoryPosition ?? int.MaxValue)
            .Select(r => r.ToResponse()));
    }

    /// <summary>Returns the logged-in participant's performance history.</summary>
    /// <response code="200">Every result the participant has, most recent event first.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">The logged-in user is not a Participant.</response>
    [HttpGet("users/me/results")]
    [RequireLogin(Roles.Participant)]
    [ProducesResponseType(typeof(List<ResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyResults()
    {
        var results = await ResultsWithDetails().Where(r => r.Enrolment.ParticipantId == CurrentUserId).ToListAsync();
        return Ok(results.OrderByDescending(r => r.Enrolment.Category.Event.EventDate).Select(r => r.ToResponse()));
    }

    // Checks that the finish time and position match the finish status. Returns an error message, or null when valid.
    private static string? ValidateResult(UpdateResultRequest request, out string finishStatus)
    {
        finishStatus = FinishStatuses.FirstOrDefault(s => s.Equals(request.FinishStatus.Trim(), StringComparison.OrdinalIgnoreCase)) ?? string.Empty;

        if (finishStatus == string.Empty)
            return "Finish status must be Finished, DNF, DNS or DSQ.";
        if (finishStatus == "Finished" && (request.FinishTime == null || request.CategoryPosition == null))
            return "A finisher needs a finish time and a category position.";
        if (finishStatus != "Finished" && (request.FinishTime != null || request.CategoryPosition != null))
            return "Only finishers can have a finish time and a category position.";

        return null;
    }
}
