using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Route("api")]
public class CategoriesController : RaceDayControllerBase
{
    private readonly RaceDayContext _db;

    public CategoriesController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>Lists the categories (distances or age groups) offered in an event.</summary>
    /// <response code="200">The event's categories with fee, start time and places remaining.</response>
    /// <response code="404">No event has this ID.</response>
    [HttpGet("events/{eventId:int}/categories")]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
            return NotFound(new MessageResponse($"Event {eventId} does not exist."));

        var categories = await _db.Categories.Include(c => c.Enrolments)
            .Where(c => c.EventId == eventId).ToListAsync();
        return Ok(categories.OrderBy(c => c.StartTime).Select(c => c.ToResponse()));
    }

    /// <summary>Adds a category to the organiser's own event, for example "10 km Road Race" or "Under 20".</summary>
    /// <response code="201">The category was created. Returns the new category.</response>
    /// <response code="400">A value is invalid, for example a distance of 0.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another organiser.</response>
    /// <response code="404">No event has this ID.</response>
    /// <response code="409">This event already has a category with this name.</response>
    [HttpPost("events/{eventId:int}/categories")]
    [RequireLogin(Roles.Organiser)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddCategory(int eventId, CategoryRequest request)
    {
        var ev = await _db.Events.Include(e => e.Categories).FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev == null)
            return NotFound(new MessageResponse($"Event {eventId} does not exist."));
        if (ev.OrganiserId != CurrentUserId)
            return Forbidden("You can only add categories to your own events.");

        var name = request.CategoryName.Trim();
        if (ev.Categories.Any(c => c.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new MessageResponse("This event already has a category with this name."));

        var category = new Category
        {
            EventId = eventId,
            CategoryName = name,
            DistanceKm = request.DistanceKm,
            StartTime = request.StartTime!.Value,
            EntryFee = request.EntryFee,
            MaxParticipants = request.MaxParticipants,
            MinimumAge = request.MinimumAge
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Created($"/api/events/{eventId}/categories", category.ToResponse());
    }

    /// <summary>Updates a category in the organiser's own event.</summary>
    /// <response code="200">The category was updated. Returns the updated category.</response>
    /// <response code="400">A value is invalid, the name is already used in this event, or the new limit is below the number of entries.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another organiser.</response>
    /// <response code="404">No category has this ID.</response>
    [HttpPut("categories/{id:int}")]
    [RequireLogin(Roles.Organiser)]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequest request)
    {
        var category = await _db.Categories
            .Include(c => c.Event).ThenInclude(e => e.Categories)
            .Include(c => c.Enrolments)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null)
            return NotFound(new MessageResponse($"Category {id} does not exist."));
        if (category.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only change categories in your own events.");

        var name = request.CategoryName.Trim();
        if (category.Event.Categories.Any(c => c.CategoryId != id && c.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            return BadRequest(new MessageResponse("This event already has a category with this name."));

        var entries = category.Enrolments.Count(e => e.Status != "Cancelled");
        if (request.MaxParticipants < entries)
            return BadRequest(new MessageResponse($"This category already has {entries} entries, so the limit cannot be lower than that."));

        category.CategoryName = name;
        category.DistanceKm = request.DistanceKm;
        category.StartTime = request.StartTime!.Value;
        category.EntryFee = request.EntryFee;
        category.MaxParticipants = request.MaxParticipants;
        category.MinimumAge = request.MinimumAge;
        await _db.SaveChangesAsync();

        return Ok(category.ToResponse());
    }

    /// <summary>Deletes a category that has no enrolments.</summary>
    /// <response code="204">The category was deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or the event belongs to another organiser.</response>
    /// <response code="404">No category has this ID.</response>
    /// <response code="409">The category has enrolments, so it cannot be deleted.</response>
    [HttpDelete("categories/{id:int}")]
    [RequireLogin(Roles.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.Include(c => c.Event).Include(c => c.Enrolments)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null)
            return NotFound(new MessageResponse($"Category {id} does not exist."));
        if (category.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only delete categories in your own events.");
        if (category.Enrolments.Any())
            return Conflict(new MessageResponse("This category has enrolments, so it cannot be deleted."));

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
