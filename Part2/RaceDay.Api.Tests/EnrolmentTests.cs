using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

// Requirement: event enrolments - Participants can enrol and the enrolment is recorded correctly
public class EnrolmentTests : ApiTestBase
{
    [Fact]
    public async Task Participant_EnrolsInOpenEvent_Returns201AndRecordsTheLink()
    {
        var organiser = await LoginAsync(Organiser);
        var (eventId, categoryId) = await CreateOpenEventWithCategoryAsync(organiser);
        var participant = await LoginAsync(Participant);

        var response = await participant.PostAsJsonAsync("/api/enrolments", new { categoryId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var enrolment = await response.Content.ReadFromJsonAsync<EnrolmentResponse>();
        Assert.Equal(eventId, enrolment!.EventId);
        Assert.Equal("Confirmed", enrolment.Status);
        Assert.False(string.IsNullOrEmpty(enrolment.RaceNumber));

        // The database links the participant, the selected category and (through it) the event
        var saved = InDatabase(db => db.Enrolments
            .Where(e => e.EnrolmentId == enrolment.EnrolmentId)
            .Select(e => new { e.ParticipantId, e.CategoryId, e.Category.EventId, e.Status })
            .Single());
        Assert.Equal(3, saved.ParticipantId);
        Assert.Equal(categoryId, saved.CategoryId);
        Assert.Equal(eventId, saved.EventId);
        Assert.Equal("Confirmed", saved.Status);
    }

    [Fact]
    public async Task Enrol_SameCategoryTwice_Returns409()
    {
        var organiser = await LoginAsync(Organiser);
        var (_, categoryId) = await CreateOpenEventWithCategoryAsync(organiser);
        var participant = await LoginAsync(Participant);
        await participant.PostAsJsonAsync("/api/enrolments", new { categoryId });

        var second = await participant.PostAsJsonAsync("/api/enrolments", new { categoryId });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Enrol_EventNotOpen_Returns400()
    {
        var participant = await LoginAsync(Participant); // category 2 belongs to the Completed event 1

        var response = await participant.PostAsJsonAsync("/api/enrolments", new { categoryId = 2 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_UnknownCategory_Returns404()
    {
        var participant = await LoginAsync(Participant);

        var response = await participant.PostAsJsonAsync("/api/enrolments", new { categoryId = 999 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_BelowMinimumAge_Returns400()
    {
        var organiser = await LoginAsync(Organiser);
        var (_, categoryId) = await CreateOpenEventWithCategoryAsync(organiser, minimumAge: 60);
        var participant = await LoginAsync(Participant); // born 1990

        var response = await participant.PostAsJsonAsync("/api/enrolments", new { categoryId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Participant_SeesOwnEnrolments()
    {
        var participant = await LoginAsync(Participant);

        var enrolments = await participant.GetFromJsonAsync<List<EnrolmentResponse>>("/api/users/me/enrolments");

        Assert.Equal(2, enrolments!.Count); // Sipho's seeded entries
        Assert.All(enrolments, e => Assert.Equal(3, e.ParticipantId));
    }

    [Fact]
    public async Task Organiser_SeesEnrolmentsForOwnEvent()
    {
        var organiser = await LoginAsync(Organiser);
        var (eventId, categoryId) = await CreateOpenEventWithCategoryAsync(organiser);
        var participant = await LoginAsync(Participant);
        await participant.PostAsJsonAsync("/api/enrolments", new { categoryId });

        var entrants = await organiser.GetFromJsonAsync<List<EntrantResponse>>($"/api/events/{eventId}/enrolments");

        var entrant = Assert.Single(entrants!);
        Assert.Equal(Participant, entrant.Email);
    }

    [Fact]
    public async Task Participant_CancelsOwnEnrolment_Returns204AndKeepsHistory()
    {
        var participant = await LoginAsync(Participant); // enrolment 8 belongs to Sipho

        var response = await participant.DeleteAsync("/api/enrolments/8");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Cancelled", InDatabase(db => db.Enrolments.Single(e => e.EnrolmentId == 8).Status));
    }

    [Fact]
    public async Task Participant_CannotCancelSomeoneElsesEnrolment_Returns403()
    {
        var participant = await LoginAsync(OtherParticipant); // enrolment 8 belongs to Sipho

        var response = await participant.DeleteAsync("/api/enrolments/8");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("Confirmed", InDatabase(db => db.Enrolments.Single(e => e.EnrolmentId == 8).Status));
    }
}
