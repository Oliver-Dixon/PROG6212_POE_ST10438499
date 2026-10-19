using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos;

// Turns database entities into the JSON responses the API returns
public static class DtoMappings
{
    public static UserProfileResponse ToResponse(this User u) => new(
        u.UserId, u.FirstName, u.LastName, u.Email, u.Role.RoleName, u.PhoneNumber,
        u.DateOfBirth, u.City, u.EmergencyContactName, u.EmergencyContactPhone, u.CreatedAt);

    public static EventTypeResponse ToResponse(this EventType t) => new(t.EventTypeId, t.TypeName, t.Description);

    public static VenueResponse ToResponse(this Venue v) => new(
        v.VenueId, v.VenueName, v.AddressLine, v.City, v.Province, v.Latitude, v.Longitude);

    public static EventResponse ToResponse(this Event e) => new(
        e.EventId, e.EventName, e.Description, e.EventDate, e.RegistrationDeadline, e.Status,
        e.EventType.TypeName, e.OrganiserId, $"{e.Organiser.FirstName} {e.Organiser.LastName}",
        e.Venue.ToResponse(),
        e.Categories.Select(c => c.DistanceKm).Distinct().OrderByDescending(d => d).ToList(),
        e.Categories.OrderBy(c => c.StartTime).Select(c => c.ToResponse()).ToList());

    public static CategoryResponse ToResponse(this Category c) => new(
        c.CategoryId, c.EventId, c.CategoryName, c.DistanceKm, c.StartTime, c.EntryFee, c.MaxParticipants, c.MinimumAge,
        c.MaxParticipants - c.Enrolments.Count(e => e.Status != "Cancelled"));

    public static EnrolmentResponse ToResponse(this Enrolment e) => new(
        e.EnrolmentId, e.ParticipantId, e.RaceNumber, e.Status, e.EnrolledAt,
        e.Category.EventId, e.Category.Event.EventName, e.Category.Event.EventDate,
        e.CategoryId, e.Category.CategoryName, e.Category.DistanceKm);

    public static EntrantResponse ToEntrant(this Enrolment e) => new(
        e.EnrolmentId, e.ParticipantId, $"{e.Participant.FirstName} {e.Participant.LastName}", e.Participant.Email,
        e.CategoryId, e.Category.CategoryName, e.RaceNumber, e.Status);

    public static ResultResponse ToResponse(this Result r) => new(
        r.ResultId, r.EnrolmentId, r.Enrolment.ParticipantId,
        $"{r.Enrolment.Participant.FirstName} {r.Enrolment.Participant.LastName}",
        r.Enrolment.Category.EventId, r.Enrolment.Category.Event.EventName, r.Enrolment.Category.Event.EventDate,
        r.Enrolment.Category.CategoryName, r.Enrolment.Category.DistanceKm,
        r.FinishStatus, r.FinishTime, r.CategoryPosition);
}
