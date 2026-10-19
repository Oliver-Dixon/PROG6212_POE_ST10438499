using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Controllers;

[Route("api")]
public class LookupsController : RaceDayControllerBase
{
    private readonly RaceDayContext _db;

    public LookupsController(RaceDayContext db)
    {
        _db = db;
    }

    /// <summary>Lists the event types (Road Running, Trail Running, Walking, Cycling) for the create-event form.</summary>
    /// <response code="200">All event types.</response>
    [HttpGet("event-types")]
    [ProducesResponseType(typeof(List<EventTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventTypes()
    {
        var types = await _db.EventTypes.OrderBy(t => t.EventTypeId).ToListAsync();
        return Ok(types.Select(t => t.ToResponse()));
    }

    /// <summary>Lists the venues (event locations) for the create-event form.</summary>
    /// <response code="200">All venues with their city, province and coordinates.</response>
    [HttpGet("venues")]
    [ProducesResponseType(typeof(List<VenueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVenues()
    {
        var venues = await _db.Venues.OrderBy(v => v.VenueName).ToListAsync();
        return Ok(venues.Select(v => v.ToResponse()));
    }
}
