using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

// Shared setup for every test class.
// xUnit creates a new instance for each test, so every test gets its own API and its own clean database.
public abstract class ApiTestBase : IDisposable
{
    // Seeded accounts from the Part 1 SQL script (all use the same password)
    protected const string Password = "RaceDay@2026";
    protected const string Organiser = "thandiwe.nkosi@example.com";        // UserId 1, owns events 1 and 4
    protected const string OtherOrganiser = "pieter.vandermerwe@example.com"; // UserId 2, owns events 2 and 3
    protected const string Participant = "sipho.dlamini@example.com";       // UserId 3
    protected const string OtherParticipant = "ayesha.patel@example.com";   // UserId 4

    protected readonly RaceDayApiFactory Factory = new();

    // A client with no session (not logged in)
    protected HttpClient AnonymousClient() => Factory.CreateClient();

    // A client that has logged in. It keeps the session cookie, like a browser.
    protected async Task<HttpClient> LoginAsync(string email, string password = Password)
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return client;
    }

    // Creates an event in the future, opens it for entries and adds one category.
    protected async Task<(int EventId, int CategoryId)> CreateOpenEventWithCategoryAsync(HttpClient organiser, int minimumAge = 0)
    {
        var eventDate = DateOnly.FromDateTime(DateTime.Today).AddDays(60);
        var newEvent = new
        {
            eventName = "Test Park Run " + Guid.NewGuid().ToString("N")[..6],
            description = "Created by a unit test",
            eventTypeId = 1,
            venueId = 1,
            eventDate,
            registrationDeadline = eventDate.AddDays(-7)
        };

        var created = await organiser.PostAsJsonAsync("/api/events", newEvent);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var ev = (await created.Content.ReadFromJsonAsync<EventResponse>())!;

        var opened = await organiser.PutAsJsonAsync($"/api/events/{ev.EventId}", new
        {
            newEvent.eventName, newEvent.description, newEvent.eventTypeId, newEvent.venueId,
            newEvent.eventDate, newEvent.registrationDeadline, status = "Open"
        });
        Assert.Equal(HttpStatusCode.OK, opened.StatusCode);

        var category = await organiser.PostAsJsonAsync($"/api/events/{ev.EventId}/categories", new
        {
            categoryName = "5 km Run",
            distanceKm = 5,
            startTime = "07:00:00",
            entryFee = 50,
            maxParticipants = 100,
            minimumAge
        });
        Assert.Equal(HttpStatusCode.Created, category.StatusCode);
        var cat = (await category.Content.ReadFromJsonAsync<CategoryResponse>())!;

        return (ev.EventId, cat.CategoryId);
    }

    // Lets a test look directly at the database to prove data was saved correctly
    protected T InDatabase<T>(Func<RaceDayContext, T> query)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
        return query(db);
    }

    public void Dispose()
    {
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
