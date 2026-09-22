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