using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

// Requirement: different roles are rejected when they try to use the other role's features
public class RoleAccessTests : ApiTestBase
{
    [Theory]
    [InlineData("GET", "/api/users/me")]
    [InlineData("PUT", "/api/users/me")]
    [InlineData("POST", "/api/events")]
    [InlineData("PUT", "/api/events/1")]
    [InlineData("DELETE", "/api/events/1")]
    [InlineData("POST", "/api/events/1/categories")]
    [InlineData("POST", "/api/enrolments")]
    [InlineData("GET", "/api/users/me/enrolments")]
    [InlineData("GET", "/api/events/1/enrolments")]
    [InlineData("POST", "/api/results")]
    [InlineData("GET", "/api/users/me/results")]
    public async Task ProtectedEndpoints_WithoutLogin_Return401(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { });

        var response = await AnonymousClient().SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent_Returns403()
    {
        var client = await LoginAsync(Participant);

        var response = await client.PostAsJsonAsync("/api/events", new
        {
            eventName = "Not allowed",
            eventTypeId = 1,
            venueId = 1,
            eventDate = DateOnly.FromDateTime(DateTime.Today).AddDays(30),
            registrationDeadline = DateOnly.FromDateTime(DateTime.Today).AddDays(20)
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotDeleteEvent_Returns403()
    {
        var client = await LoginAsync(Participant);

        var response = await client.DeleteAsync("/api/events/4");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotAddCategory_Returns403()
    {
        var client = await LoginAsync(Participant);

        var response = await client.PostAsJsonAsync("/api/events/4/categories", new
        {
            categoryName = "Sneaky",
            distanceKm = 1,
            startTime = "08:00:00",
            maxParticipants = 10
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotViewEventEntries_Returns403()
    {
        var client = await LoginAsync(Participant);

        var response = await client.GetAsync("/api/events/1/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotRecordResult_Returns403()
    {
        var client = await LoginAsync(Participant);

        var response = await client.PostAsJsonAsync("/api/results", new
        {
            enrolmentId = 8,
            finishStatus = "Finished",
            finishTime = "00:59:00",
            categoryPosition = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotEnrol_Returns403()
    {
        var client = await LoginAsync(Organiser);

        var response = await client.PostAsJsonAsync("/api/enrolments", new { categoryId = 6 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotViewParticipantResultHistory_Returns403()
    {
        var client = await LoginAsync(Organiser);

        var response = await client.GetAsync("/api/users/me/results");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotViewAnotherOrganisersEntries_Returns403()
    {
        var client = await LoginAsync(Organiser); // event 2 belongs to Pieter

        var response = await client.GetAsync("/api/events/2/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
