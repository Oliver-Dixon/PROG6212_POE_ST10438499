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