using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;

// Results - Organisers capture finish times and positions, Participants view their own results
public class ResultTests : ApiTestBase
{
    [Fact]
    public async Task Organiser_RecordsResultAfterTheEvent_Returns201()
    {
        var organiser = await LoginAsync(Organiser);
        var (eventId, categoryId) = await CreateOpenEventWithCategoryAsync(organiser);
        var participant = await LoginAsync(Participant);
        var enrol = await participant.PostAsJsonAsync("/api/enrolments", new { categoryId });
        var enrolment = (await enrol.Content.ReadFromJsonAsync<EnrolmentResponse>())!;

        // Move the event into the past so it has "taken place"
        InDatabase(db =>
        {
            var ev = db.Events.Single(e => e.EventId == eventId);
            ev.EventDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
            ev.RegistrationDeadline = ev.EventDate.AddDays(-7);
            return db.SaveChanges();
        });

        var response = await organiser.PostAsJsonAsync("/api/results", new
        {
            enrolmentId = enrolment.EnrolmentId,
            finishStatus = "Finished",
            finishTime = "00:24:31",
            categoryPosition = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var myResults = await participant.GetFromJsonAsync<List<ResultResponse>>("/api/users/me/results");
        Assert.Contains(myResults!, r => r.EventId == eventId && r.FinishTime == new TimeOnly(0, 24, 31));
    }

    [Fact]
    public async Task RecordResult_FinisherWithoutTime_Returns400()
    {
        var organiser = await LoginAsync(Organiser);

        var response = await organiser.PostAsJsonAsync("/api/results", new { enrolmentId = 1, finishStatus = "Finished" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RecordResult_AlreadyRecorded_Returns409()
    {
        var organiser = await LoginAsync(Organiser);

        var response = await organiser.PostAsJsonAsync("/api/results", new
        {
            enrolmentId = 1,
            finishStatus = "Finished",
            finishTime = "01:30:00",
            categoryPosition = 1
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateResult_ByOwner_Returns200()
    {
        var organiser = await LoginAsync(Organiser); // result 3 is Ryan's DNF in event 1

        var response = await organiser.PutAsJsonAsync("/api/results/3", new
        {
            finishStatus = "Finished",
            finishTime = "02:05:00",
            categoryPosition = 3
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Finished", InDatabase(db => db.Results.Single(r => r.ResultId == 3).FinishStatus));
    }

    [Fact]
    public async Task Participant_SeesOwnResults()
    {
        var participant = await LoginAsync(Participant);

        var results = await participant.GetFromJsonAsync<List<ResultResponse>>("/api/users/me/results");

        var result = Assert.Single(results!);
        Assert.Equal(new TimeOnly(1, 38, 42), result.FinishTime);
        Assert.Equal(1, result.CategoryPosition);
    }

    [Fact]
    public async Task EventResults_ArePublic()
    {
        var results = await AnonymousClient().GetFromJsonAsync<List<ResultResponse>>("/api/events/1/results");

        Assert.Equal(6, results!.Count);
        Assert.Equal("DNF", results.First(r => r.CategoryName == "21.1 km Half Marathon" && r.CategoryPosition == null).FinishStatus);
    }
}
