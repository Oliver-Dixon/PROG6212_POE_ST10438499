using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

// Requirement: event management - Organisers can create, update and delete their own events
public class EventTests : ApiTestBase
{
    private static object NewEvent(int daysFromToday = 30, int deadlineDaysBefore = 7)
    {
        var eventDate = DateOnly.FromDateTime(DateTime.Today).AddDays(daysFromToday);
        return new
        {
            eventName = "Sea Point Promenade 10K",
            description = "Flat out-and-back route along the promenade.",
            eventTypeId = 1,
            venueId = 2,
            eventDate,
            registrationDeadline = eventDate.AddDays(-deadlineDaysBefore)
        };
    }

    [Fact]
    public async Task GetEvents_WithoutLogin_Returns200AndHidesDrafts()
    {
        var events = await AnonymousClient().GetFromJsonAsync<List<EventResponse>>("/api/events");

        Assert.NotNull(events);
        Assert.Equal(3, events.Count);
        Assert.DoesNotContain(events, e => e.Status == "Draft");
    }

    [Fact]
    public async Task GetEvent_IncludesLocationDistancesAndType()
    {
        var ev = await AnonymousClient().GetFromJsonAsync<EventResponse>("/api/events/1");

        Assert.Equal("Durban Sunrise Half Marathon", ev!.EventName);
        Assert.Equal("Durban", ev.Location.City);
        Assert.Equal("Road Running", ev.EventType);
        Assert.Contains(21.10m, ev.DistancesKm);
        Assert.Equal(3, ev.Categories.Count);
    }

    [Fact]
    public async Task GetEvent_UnknownId_Returns404()
    {
        var response = await AnonymousClient().GetAsync("/api/events/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_Returns201AsDraft()
    {
        var client = await LoginAsync(Organiser);

        var response = await client.PostAsJsonAsync("/api/events", NewEvent());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<EventResponse>();
        Assert.Equal("Draft", created!.Status);
        Assert.Equal(1, created.OrganiserId);
        Assert.True(InDatabase(db => db.Events.Any(e => e.EventId == created.EventId)));
    }

    [Fact]
    public async Task CreateEvent_WithoutLogin_Returns401()
    {
        var response = await AnonymousClient().PostAsJsonAsync("/api/events", NewEvent());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_DeadlineAfterEventDate_Returns400()
    {
        var client = await LoginAsync(Organiser);

        var response = await client.PostAsJsonAsync("/api/events", NewEvent(deadlineDaysBefore: -3));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_DateInThePast_Returns400()
    {
        var client = await LoginAsync(Organiser);

        var response = await client.PostAsJsonAsync("/api/events", NewEvent(daysFromToday: -10));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEvent_OwnEvent_Returns200AndSavesStatus()
    {
        var client = await LoginAsync(Organiser);

        var response = await client.PutAsJsonAsync("/api/events/4", new
        {
            eventName = "Soweto Reconciliation Walk",
            description = "Updated description",
            eventTypeId = 3,
            venueId = 4,
            eventDate = "2026-12-16",
            registrationDeadline = "2026-12-09",
            status = "Open"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Open", InDatabase(db => db.Events.Single(e => e.EventId == 4).Status));
    }

    [Fact]
    public async Task UpdateEvent_AnotherOrganisersEvent_Returns403()
    {
        var client = await LoginAsync(Organiser); // event 2 belongs to Pieter

        var response = await client.PutAsJsonAsync("/api/events/2", new
        {
            eventName = "Taken over",
            eventTypeId = 2,
            venueId = 2,
            eventDate = "2026-10-25",
            registrationDeadline = "2026-10-18",
            status = "Cancelled"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("Silvermine Trail Challenge", InDatabase(db => db.Events.Single(e => e.EventId == 2).EventName));
    }

    [Fact]
    public async Task DeleteEvent_OwnEventWithNoEntries_Returns204()
    {
        var client = await LoginAsync(Organiser); // event 4 has categories but no enrolments

        var response = await client.DeleteAsync("/api/events/4");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/events/4")).StatusCode);
    }

    [Fact]
    public async Task DeleteEvent_WithEntries_Returns409()
    {
        var client = await LoginAsync(Organiser); // event 1 has enrolments

        var response = await client.DeleteAsync("/api/events/1");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddCategory_ToOwnEvent_Returns201AndIsListed()
    {
        var client = await LoginAsync(Organiser);

        var response = await client.PostAsJsonAsync("/api/events/4/categories", new
        {
            categoryName = "Under 20",
            distanceKm = 5,
            startTime = "09:00:00",
            entryFee = 0,
            maxParticipants = 200,
            minimumAge = 0
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var categories = await AnonymousClient().GetFromJsonAsync<List<CategoryResponse>>("/api/events/4/categories");
        Assert.Contains(categories!, c => c.CategoryName == "Under 20");
    }
}
