using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

// The same sample data as the Part 1 SQL script.
// Every sample user's password is RaceDay@2026 (stored as a BCrypt hash).
public static class SeedData
{
    private const string PasswordHash = "$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO";
    private static readonly DateTime Created = new(2026, 6, 1);

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Organiser", Description = "Creates and manages events, categories and participant results." },
            new Role { RoleId = 2, RoleName = "Participant", Description = "Browses events, enrols in categories and tracks personal performance history." });

        modelBuilder.Entity<User>().HasData(
            new User { UserId = 1, RoleId = 1, FirstName = "Thandiwe", LastName = "Nkosi", Email = "thandiwe.nkosi@example.com", PasswordHash = PasswordHash, PhoneNumber = "+27825550101", City = "Durban", CreatedAt = Created },
            new User { UserId = 2, RoleId = 1, FirstName = "Pieter", LastName = "van der Merwe", Email = "pieter.vandermerwe@example.com", PasswordHash = PasswordHash, PhoneNumber = "+27835550102", City = "Stellenbosch", CreatedAt = Created },
            new User { UserId = 3, RoleId = 2, FirstName = "Sipho", LastName = "Dlamini", Email = "sipho.dlamini@example.com", PasswordHash = PasswordHash, PhoneNumber = "+27715550103", DateOfBirth = new DateOnly(1990, 4, 12), City = "Johannesburg", EmergencyContactName = "Nomsa Dlamini", EmergencyContactPhone = "+27715550203", CreatedAt = Created },
            new User { UserId = 4, RoleId = 2, FirstName = "Ayesha", LastName = "Patel", Email = "ayesha.patel@example.com", PasswordHash = PasswordHash, PhoneNumber = "+27845550104", DateOfBirth = new DateOnly(1995, 9, 3), City = "Durban", EmergencyContactName = "Imran Patel", EmergencyContactPhone = "+27845550204", CreatedAt = Created },
            new User { UserId = 5, RoleId = 2, FirstName = "Lerato", LastName = "Mokoena", Email = "lerato.mokoena@example.com", PasswordHash = PasswordHash, PhoneNumber = "+27725550105", DateOfBirth = new DateOnly(1988, 1, 27), City = "Pretoria", EmergencyContactName = "Kabelo Mokoena", EmergencyContactPhone = "+27725550205", CreatedAt = Created },
            new User { UserId = 6, RoleId = 2, FirstName = "Johan", LastName = "Botha", Email = "johan.botha@example.com", PasswordHash = PasswordHash, PhoneNumber = "+27825550106", DateOfBirth = new DateOnly(1979, 11, 15), City = "Cape Town", EmergencyContactName = "Marietjie Botha", EmergencyContactPhone = "+27825550206", CreatedAt = Created },
            new User { UserId = 7, RoleId = 2, FirstName = "Naledi", LastName = "Khumalo", Email = "naledi.khumalo@example.com", PasswordHash = PasswordHash, PhoneNumber = "+27765550107", DateOfBirth = new DateOnly(2001, 6, 30), City = "Soweto", EmergencyContactName = "Zanele Khumalo", EmergencyContactPhone = "+27765550207", CreatedAt = Created },
            new User { UserId = 8, RoleId = 2, FirstName = "Ryan", LastName = "Naidoo", Email = "ryan.naidoo@example.com", PasswordHash = PasswordHash, PhoneNumber = "+27615550108", DateOfBirth = new DateOnly(1993, 2, 8), City = "Durban", EmergencyContactName = "Priya Naidoo", EmergencyContactPhone = "+27615550208", CreatedAt = Created });

        modelBuilder.Entity<EventType>().HasData(
            new EventType { EventTypeId = 1, TypeName = "Road Running", Description = "Tarred road races from 5 km fun runs to ultra-marathons." },
            new EventType { EventTypeId = 2, TypeName = "Trail Running", Description = "Off-road running on mountain, forest and nature reserve trails." },
            new EventType { EventTypeId = 3, TypeName = "Walking", Description = "Community, heritage and charity walks for all ages." },
            new EventType { EventTypeId = 4, TypeName = "Cycling", Description = "Road cycling races and fun rides." });

        modelBuilder.Entity<Venue>().HasData(
            new Venue { VenueId = 1, VenueName = "Durban Beachfront Promenade", AddressLine = "O R Tambo Parade", City = "Durban", Province = "KwaZulu-Natal", Latitude = -29.858700m, Longitude = 31.043000m },
            new Venue { VenueId = 2, VenueName = "Silvermine Nature Reserve", AddressLine = "Ou Kaapse Weg", City = "Cape Town", Province = "Western Cape", Latitude = -34.083300m, Longitude = 18.420000m },
            new Venue { VenueId = 3, VenueName = "Stellenbosch Town Centre", AddressLine = "Die Braak, Bird St", City = "Stellenbosch", Province = "Western Cape", Latitude = -33.936800m, Longitude = 18.860200m },
            new Venue { VenueId = 4, VenueName = "Vilakazi Street Precinct", AddressLine = "Vilakazi St, Orlando", City = "Soweto", Province = "Gauteng", Latitude = -26.238600m, Longitude = 27.908100m });

        modelBuilder.Entity<Event>().HasData(
            new Event { EventId = 1, OrganiserId = 1, EventTypeId = 1, VenueId = 1, EventName = "Durban Sunrise Half Marathon", Description = "A flat, fast coastal route along the Golden Mile, starting at sunrise.", EventDate = new DateOnly(2026, 8, 16), RegistrationDeadline = new DateOnly(2026, 8, 9), Status = "Completed", CreatedAt = Created },
            new Event { EventId = 2, OrganiserId = 2, EventTypeId = 2, VenueId = 2, EventName = "Silvermine Trail Challenge", Description = "Single-track mountain trails with views over False Bay and the Atlantic.", EventDate = new DateOnly(2026, 10, 25), RegistrationDeadline = new DateOnly(2026, 10, 18), Status = "Open", CreatedAt = Created },
            new Event { EventId = 3, OrganiserId = 2, EventTypeId = 4, VenueId = 3, EventName = "Winelands Cycle Classic", Description = "A scenic road ride through the Stellenbosch and Franschhoek wine valleys.", EventDate = new DateOnly(2026, 11, 15), RegistrationDeadline = new DateOnly(2026, 11, 1), Status = "Open", CreatedAt = Created },
            new Event { EventId = 4, OrganiserId = 1, EventTypeId = 3, VenueId = 4, EventName = "Soweto Reconciliation Walk", Description = "A community heritage walk past Vilakazi Street landmarks on Reconciliation Day.", EventDate = new DateOnly(2026, 12, 16), RegistrationDeadline = new DateOnly(2026, 12, 9), Status = "Draft", CreatedAt = Created });

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, EventId = 1, CategoryName = "21.1 km Half Marathon", DistanceKm = 21.10m, StartTime = new TimeOnly(5, 30), EntryFee = 250.00m, MaxParticipants = 1500, MinimumAge = 16 },
            new Category { CategoryId = 2, EventId = 1, CategoryName = "10 km Road Race", DistanceKm = 10.00m, StartTime = new TimeOnly(6, 0), EntryFee = 150.00m, MaxParticipants = 2000, MinimumAge = 12 },
            new Category { CategoryId = 3, EventId = 1, CategoryName = "5 km Fun Walk", DistanceKm = 5.00m, StartTime = new TimeOnly(6, 30), EntryFee = 60.00m, MaxParticipants = 1000, MinimumAge = 0 },
            new Category { CategoryId = 4, EventId = 2, CategoryName = "22 km Trail Run", DistanceKm = 22.00m, StartTime = new TimeOnly(6, 30), EntryFee = 380.00m, MaxParticipants = 300, MinimumAge = 18 },
            new Category { CategoryId = 5, EventId = 2, CategoryName = "12 km Trail Run", DistanceKm = 12.00m, StartTime = new TimeOnly(7, 0), EntryFee = 280.00m, MaxParticipants = 400, MinimumAge = 14 },
            new Category { CategoryId = 6, EventId = 3, CategoryName = "105 km Road Race", DistanceKm = 105.00m, StartTime = new TimeOnly(6, 0), EntryFee = 650.00m, MaxParticipants = 1200, MinimumAge = 16 },
            new Category { CategoryId = 7, EventId = 3, CategoryName = "50 km Fun Ride", DistanceKm = 50.00m, StartTime = new TimeOnly(7, 30), EntryFee = 350.00m, MaxParticipants = 800, MinimumAge = 12 },
            new Category { CategoryId = 8, EventId = 4, CategoryName = "8 km Heritage Walk", DistanceKm = 8.00m, StartTime = new TimeOnly(8, 0), EntryFee = 80.00m, MaxParticipants = 1500, MinimumAge = 0 },
            new Category { CategoryId = 9, EventId = 4, CategoryName = "3 km Family Walk", DistanceKm = 3.00m, StartTime = new TimeOnly(8, 30), EntryFee = 0m, MaxParticipants = 500, MinimumAge = 0 });

        modelBuilder.Entity<Enrolment>().HasData(
            new Enrolment { EnrolmentId = 1, ParticipantId = 3, CategoryId = 1, RaceNumber = "DSH1001", Status = "Confirmed", EnrolledAt = new DateTime(2026, 6, 2, 9, 15, 0) },
            new Enrolment { EnrolmentId = 2, ParticipantId = 4, CategoryId = 1, RaceNumber = "DSH1002", Status = "Confirmed", EnrolledAt = new DateTime(2026, 6, 10, 18, 42, 0) },
            new Enrolment { EnrolmentId = 3, ParticipantId = 8, CategoryId = 1, RaceNumber = "DSH1003", Status = "Confirmed", EnrolledAt = new DateTime(2026, 7, 1, 7, 5, 0) },
            new Enrolment { EnrolmentId = 4, ParticipantId = 5, CategoryId = 2, RaceNumber = "DSH2001", Status = "Confirmed", EnrolledAt = new DateTime(2026, 6, 15, 12, 30, 0) },
            new Enrolment { EnrolmentId = 5, ParticipantId = 7, CategoryId = 2, RaceNumber = "DSH2002", Status = "Confirmed", EnrolledAt = new DateTime(2026, 7, 20, 20, 11, 0) },
            new Enrolment { EnrolmentId = 6, ParticipantId = 6, CategoryId = 3, RaceNumber = "DSH3001", Status = "Confirmed", EnrolledAt = new DateTime(2026, 8, 1, 10, 0, 0) },
            new Enrolment { EnrolmentId = 7, ParticipantId = 6, CategoryId = 4, RaceNumber = "SMT0101", Status = "Confirmed", EnrolledAt = new DateTime(2026, 8, 20, 8, 25, 0) },
            new Enrolment { EnrolmentId = 8, ParticipantId = 3, CategoryId = 5, RaceNumber = "SMT0201", Status = "Confirmed", EnrolledAt = new DateTime(2026, 9, 1, 19, 40, 0) },
            new Enrolment { EnrolmentId = 9, ParticipantId = 4, CategoryId = 5, RaceNumber = "SMT0202", Status = "Cancelled", EnrolledAt = new DateTime(2026, 9, 5, 13, 5, 0) },
            new Enrolment { EnrolmentId = 10, ParticipantId = 5, CategoryId = 6, RaceNumber = "WCC0001", Status = "Confirmed", EnrolledAt = new DateTime(2026, 9, 10, 6, 50, 0) },
            new Enrolment { EnrolmentId = 11, ParticipantId = 8, CategoryId = 7, RaceNumber = "WCC5001", Status = "Confirmed", EnrolledAt = new DateTime(2026, 9, 12, 21, 15, 0) },
            new Enrolment { EnrolmentId = 12, ParticipantId = 7, CategoryId = 7, RaceNumber = null, Status = "Pending", EnrolledAt = new DateTime(2026, 9, 15, 16, 30, 0) });

        modelBuilder.Entity<Result>().HasData(
            new Result { ResultId = 1, EnrolmentId = 1, FinishStatus = "Finished", FinishTime = new TimeOnly(1, 38, 42), CategoryPosition = 1, RecordedAt = new DateTime(2026, 8, 16, 13, 0, 0) },
            new Result { ResultId = 2, EnrolmentId = 2, FinishStatus = "Finished", FinishTime = new TimeOnly(1, 52, 10), CategoryPosition = 2, RecordedAt = new DateTime(2026, 8, 16, 13, 0, 0) },
            new Result { ResultId = 3, EnrolmentId = 3, FinishStatus = "DNF", FinishTime = null, CategoryPosition = null, RecordedAt = new DateTime(2026, 8, 16, 13, 0, 0) },
            new Result { ResultId = 4, EnrolmentId = 4, FinishStatus = "Finished", FinishTime = new TimeOnly(0, 44, 5), CategoryPosition = 1, RecordedAt = new DateTime(2026, 8, 16, 13, 0, 0) },
            new Result { ResultId = 5, EnrolmentId = 5, FinishStatus = "Finished", FinishTime = new TimeOnly(0, 51, 30), CategoryPosition = 2, RecordedAt = new DateTime(2026, 8, 16, 13, 0, 0) },
            new Result { ResultId = 6, EnrolmentId = 6, FinishStatus = "Finished", FinishTime = new TimeOnly(0, 38, 20), CategoryPosition = 1, RecordedAt = new DateTime(2026, 8, 16, 13, 0, 0) });
    }
}
