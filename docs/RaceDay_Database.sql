IF DB_ID('RaceDayDB') IS NULL CREATE DATABASE RaceDayDB;
GO
USE RaceDayDB;
GO
DROP TABLE IF EXISTS Results, Enrolments, Categories, Events, Venues, EventTypes, Users, Roles;
GO

CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(200)
);
GO

CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    RoleId INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleId),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20),
    DateOfBirth DATE,
    City NVARCHAR(100),
    EmergencyContactName NVARCHAR(100),
    EmergencyContactPhone NVARCHAR(20),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE EventTypes (
    EventTypeId INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200)
);
GO

CREATE TABLE Venues (
    VenueId INT IDENTITY(1,1) PRIMARY KEY,
    VenueName NVARCHAR(150) NOT NULL,
    AddressLine NVARCHAR(200),
    City NVARCHAR(100) NOT NULL,
    Province NVARCHAR(50) NOT NULL,
    Latitude DECIMAL(9,6) NOT NULL,
    Longitude DECIMAL(9,6) NOT NULL,
    UNIQUE (VenueName, City)
);
GO

CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    EventTypeId INT NOT NULL FOREIGN KEY REFERENCES EventTypes(EventTypeId),
    VenueId INT NOT NULL FOREIGN KEY REFERENCES Venues(VenueId),
    EventName NVARCHAR(150) NOT NULL,
    Description NVARCHAR(2000),
    EventDate DATE NOT NULL,
    RegistrationDeadline DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft' CHECK (Status IN ('Draft', 'Open', 'Closed', 'Completed', 'Cancelled')),
    ImageUrl NVARCHAR(500),
    RouteMapUrl NVARCHAR(500),
    CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0),
    UNIQUE (EventName, EventDate),
    CHECK (RegistrationDeadline <= EventDate)
);
GO

CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL FOREIGN KEY REFERENCES Events(EventId) ON DELETE CASCADE,
    CategoryName NVARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL CHECK (DistanceKm > 0),
    StartTime TIME(0) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK (EntryFee >= 0),
    MaxParticipants INT NOT NULL CHECK (MaxParticipants > 0),
    MinimumAge TINYINT NOT NULL DEFAULT 0,
    UNIQUE (EventId, CategoryName)
);
GO

CREATE TABLE Enrolments (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES Categories(CategoryId),
    RaceNumber NVARCHAR(10),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Confirmed' CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),
    EnrolledAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    UNIQUE (ParticipantId, CategoryId)
);
GO

CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Enrolments(EnrolmentId) ON DELETE CASCADE,
    FinishStatus NVARCHAR(10) NOT NULL DEFAULT 'Finished' CHECK (FinishStatus IN ('Finished', 'DNF', 'DNS', 'DSQ')),
    FinishTime TIME(0),
    CategoryPosition INT CHECK (CategoryPosition > 0),
    RecordedAt DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CHECK ((FinishStatus = 'Finished' AND FinishTime IS NOT NULL AND CategoryPosition IS NOT NULL)
        OR (FinishStatus <> 'Finished' AND FinishTime IS NULL AND CategoryPosition IS NULL))
);
GO

INSERT INTO Roles (RoleName, Description) VALUES
('Organiser', 'Creates and manages events, categories and participant results.'),
('Participant', 'Browses events, enrols in categories and tracks personal performance history.');
GO

DECLARE @pw NVARCHAR(255) = '$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO';
INSERT INTO Users (RoleId, FirstName, LastName, Email, PasswordHash, PhoneNumber, DateOfBirth, City, EmergencyContactName, EmergencyContactPhone) VALUES
(1, 'Thandiwe', 'Nkosi', 'thandiwe.nkosi@example.com', @pw, '+27825550101', NULL, 'Durban', NULL, NULL),
(1, 'Pieter', 'van der Merwe', 'pieter.vandermerwe@example.com', @pw, '+27835550102', NULL, 'Stellenbosch', NULL, NULL),
(2, 'Sipho', 'Dlamini', 'sipho.dlamini@example.com', @pw, '+27715550103', '1990-04-12', 'Johannesburg', 'Nomsa Dlamini', '+27715550203'),
(2, 'Ayesha', 'Patel', 'ayesha.patel@example.com', @pw, '+27845550104', '1995-09-03', 'Durban', 'Imran Patel', '+27845550204'),
(2, 'Lerato', 'Mokoena', 'lerato.mokoena@example.com', @pw, '+27725550105', '1988-01-27', 'Pretoria', 'Kabelo Mokoena', '+27725550205'),
(2, 'Johan', 'Botha', 'johan.botha@example.com', @pw, '+27825550106', '1979-11-15', 'Cape Town', 'Marietjie Botha', '+27825550206'),
(2, 'Naledi', 'Khumalo', 'naledi.khumalo@example.com', @pw, '+27765550107', '2001-06-30', 'Soweto', 'Zanele Khumalo', '+27765550207'),
(2, 'Ryan', 'Naidoo', 'ryan.naidoo@example.com', @pw, '+27615550108', '1993-02-08', 'Durban', 'Priya Naidoo', '+27615550208');
GO

INSERT INTO EventTypes (TypeName, Description) VALUES
('Road Running', 'Tarred road races from 5 km fun runs to ultra-marathons.'),
('Trail Running', 'Off-road running on mountain, forest and nature reserve trails.'),
('Walking', 'Community, heritage and charity walks for all ages.'),
('Cycling', 'Road cycling races and fun rides.');
GO

INSERT INTO Venues (VenueName, AddressLine, City, Province, Latitude, Longitude) VALUES
('Durban Beachfront Promenade', 'O R Tambo Parade', 'Durban', 'KwaZulu-Natal', -29.858700, 31.043000),
('Silvermine Nature Reserve', 'Ou Kaapse Weg', 'Cape Town', 'Western Cape', -34.083300, 18.420000),
('Stellenbosch Town Centre', 'Die Braak, Bird St', 'Stellenbosch', 'Western Cape', -33.936800, 18.860200),
('Vilakazi Street Precinct', 'Vilakazi St, Orlando', 'Soweto', 'Gauteng', -26.238600, 27.908100);
GO

INSERT INTO Events (OrganiserId, EventTypeId, VenueId, EventName, Description, EventDate, RegistrationDeadline, Status) VALUES
(1, 1, 1, 'Durban Sunrise Half Marathon', 'A flat, fast coastal route along the Golden Mile, starting at sunrise.', '2026-08-16', '2026-08-09', 'Completed'),
(2, 2, 2, 'Silvermine Trail Challenge', 'Single-track mountain trails with views over False Bay and the Atlantic.', '2026-10-25', '2026-10-18', 'Open'),
(2, 4, 3, 'Winelands Cycle Classic', 'A scenic road ride through the Stellenbosch and Franschhoek wine valleys.', '2026-11-15', '2026-11-01', 'Open');
GO

INSERT INTO Events (OrganiserId, EventTypeId, VenueId, EventName, Description, EventDate, RegistrationDeadline) VALUES
(1, 3, 4, 'Soweto Reconciliation Walk', 'A community heritage walk past Vilakazi Street landmarks on Reconciliation Day.', '2026-12-16', '2026-12-09');
GO

INSERT INTO Categories (EventId, CategoryName, DistanceKm, StartTime, EntryFee, MaxParticipants, MinimumAge) VALUES
(1, '21.1 km Half Marathon', 21.10, '05:30', 250.00, 1500, 16),
(1, '10 km Road Race', 10.00, '06:00', 150.00, 2000, 12),
(1, '5 km Fun Walk', 5.00, '06:30', 60.00, 1000, 0),
(2, '22 km Trail Run', 22.00, '06:30', 380.00, 300, 18),
(2, '12 km Trail Run', 12.00, '07:00', 280.00, 400, 14),
(3, '105 km Road Race', 105.00, '06:00', 650.00, 1200, 16),
(3, '50 km Fun Ride', 50.00, '07:30', 350.00, 800, 12),
(4, '8 km Heritage Walk', 8.00, '08:00', 80.00, 1500, 0);
GO

INSERT INTO Categories (EventId, CategoryName, DistanceKm, StartTime, MaxParticipants) VALUES
(4, '3 km Family Walk', 3.00, '08:30', 500);
GO