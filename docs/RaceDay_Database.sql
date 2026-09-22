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