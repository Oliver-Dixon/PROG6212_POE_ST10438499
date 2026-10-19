using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RaceDay.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    VenueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.VenueId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganiserId = table.Column<int>(type: "int", nullable: false),
                    EventTypeId = table.Column<int>(type: "int", nullable: false),
                    VenueId = table.Column<int>(type: "int", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RegistrationDeadline = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RouteMapUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.CheckConstraint("CK_Events_RegistrationDeadline", "[RegistrationDeadline] <= [EventDate]");
                    table.CheckConstraint("CK_Events_Status", "[Status] IN ('Draft', 'Open', 'Closed', 'Completed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Events_EventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventTypes",
                        principalColumn: "EventTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Users_OrganiserId",
                        column: x => x.OrganiserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    EntryFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    MinimumAge = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.CheckConstraint("CK_Categories_DistanceKm", "[DistanceKm] > 0");
                    table.CheckConstraint("CK_Categories_EntryFee", "[EntryFee] >= 0");
                    table.CheckConstraint("CK_Categories_MaxParticipants", "[MaxParticipants] > 0");
                    table.ForeignKey(
                        name: "FK_Categories_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                columns: table => new
                {
                    EnrolmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    RaceNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Confirmed"),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.EnrolmentId);
                    table.CheckConstraint("CK_Enrolments_Status", "[Status] IN ('Pending', 'Confirmed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Enrolments_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrolments_Users_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrolmentId = table.Column<int>(type: "int", nullable: false),
                    FinishStatus = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Finished"),
                    FinishTime = table.Column<TimeOnly>(type: "time(0)", nullable: true),
                    CategoryPosition = table.Column<int>(type: "int", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.ResultId);
                    table.CheckConstraint("CK_Results_CategoryPosition", "[CategoryPosition] > 0");
                    table.CheckConstraint("CK_Results_FinishStatus", "[FinishStatus] IN ('Finished', 'DNF', 'DNS', 'DSQ')");
                    table.CheckConstraint("CK_Results_FinishTime_Position", "([FinishStatus] = 'Finished' AND [FinishTime] IS NOT NULL AND [CategoryPosition] IS NOT NULL) OR ([FinishStatus] <> 'Finished' AND [FinishTime] IS NULL AND [CategoryPosition] IS NULL)");
                    table.ForeignKey(
                        name: "FK_Results_Enrolments_EnrolmentId",
                        column: x => x.EnrolmentId,
                        principalTable: "Enrolments",
                        principalColumn: "EnrolmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "EventTypeId", "Description", "TypeName" },
                values: new object[,]
                {
                    { 1, "Tarred road races from 5 km fun runs to ultra-marathons.", "Road Running" },
                    { 2, "Off-road running on mountain, forest and nature reserve trails.", "Trail Running" },
                    { 3, "Community, heritage and charity walks for all ages.", "Walking" },
                    { 4, "Road cycling races and fun rides.", "Cycling" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Description", "RoleName" },
                values: new object[,]
                {
                    { 1, "Creates and manages events, categories and participant results.", "Organiser" },
                    { 2, "Browses events, enrols in categories and tracks personal performance history.", "Participant" }
                });

            migrationBuilder.InsertData(
                table: "Venues",
                columns: new[] { "VenueId", "AddressLine", "City", "Latitude", "Longitude", "Province", "VenueName" },
                values: new object[,]
                {
                    { 1, "O R Tambo Parade", "Durban", -29.858700m, 31.043000m, "KwaZulu-Natal", "Durban Beachfront Promenade" },
                    { 2, "Ou Kaapse Weg", "Cape Town", -34.083300m, 18.420000m, "Western Cape", "Silvermine Nature Reserve" },
                    { 3, "Die Braak, Bird St", "Stellenbosch", -33.936800m, 18.860200m, "Western Cape", "Stellenbosch Town Centre" },
                    { 4, "Vilakazi St, Orlando", "Soweto", -26.238600m, 27.908100m, "Gauteng", "Vilakazi Street Precinct" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "City", "CreatedAt", "DateOfBirth", "Email", "EmergencyContactName", "EmergencyContactPhone", "FirstName", "IsActive", "LastName", "PasswordHash", "PhoneNumber", "RoleId" },
                values: new object[,]
                {
                    { 1, "Durban", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "thandiwe.nkosi@example.com", null, null, "Thandiwe", true, "Nkosi", "$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO", "+27825550101", 1 },
                    { 2, "Stellenbosch", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "pieter.vandermerwe@example.com", null, null, "Pieter", true, "van der Merwe", "$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO", "+27835550102", 1 },
                    { 3, "Johannesburg", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1990, 4, 12), "sipho.dlamini@example.com", "Nomsa Dlamini", "+27715550203", "Sipho", true, "Dlamini", "$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO", "+27715550103", 2 },
                    { 4, "Durban", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1995, 9, 3), "ayesha.patel@example.com", "Imran Patel", "+27845550204", "Ayesha", true, "Patel", "$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO", "+27845550104", 2 },
                    { 5, "Pretoria", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1988, 1, 27), "lerato.mokoena@example.com", "Kabelo Mokoena", "+27725550205", "Lerato", true, "Mokoena", "$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO", "+27725550105", 2 },
                    { 6, "Cape Town", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1979, 11, 15), "johan.botha@example.com", "Marietjie Botha", "+27825550206", "Johan", true, "Botha", "$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO", "+27825550106", 2 },
                    { 7, "Soweto", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(2001, 6, 30), "naledi.khumalo@example.com", "Zanele Khumalo", "+27765550207", "Naledi", true, "Khumalo", "$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO", "+27765550107", 2 },
                    { 8, "Durban", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateOnly(1993, 2, 8), "ryan.naidoo@example.com", "Priya Naidoo", "+27615550208", "Ryan", true, "Naidoo", "$2b$11$i3K7yDdq51UrYtvGXABe5OVQFqgIGa1jfwJcsQOwtm8UFmzLhgdJO", "+27615550108", 2 }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "CreatedAt", "Description", "EventDate", "EventName", "EventTypeId", "ImageUrl", "OrganiserId", "RegistrationDeadline", "RouteMapUrl", "Status", "UpdatedAt", "VenueId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A flat, fast coastal route along the Golden Mile, starting at sunrise.", new DateOnly(2026, 8, 16), "Durban Sunrise Half Marathon", 1, null, 1, new DateOnly(2026, 8, 9), null, "Completed", null, 1 },
                    { 2, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Single-track mountain trails with views over False Bay and the Atlantic.", new DateOnly(2026, 10, 25), "Silvermine Trail Challenge", 2, null, 2, new DateOnly(2026, 10, 18), null, "Open", null, 2 },
                    { 3, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A scenic road ride through the Stellenbosch and Franschhoek wine valleys.", new DateOnly(2026, 11, 15), "Winelands Cycle Classic", 4, null, 2, new DateOnly(2026, 11, 1), null, "Open", null, 3 },
                    { 4, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A community heritage walk past Vilakazi Street landmarks on Reconciliation Day.", new DateOnly(2026, 12, 16), "Soweto Reconciliation Walk", 3, null, 1, new DateOnly(2026, 12, 9), null, "Draft", null, 4 }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "DistanceKm", "EntryFee", "EventId", "MaxParticipants", "MinimumAge", "StartTime" },
                values: new object[,]
                {
                    { 1, "21.1 km Half Marathon", 21.10m, 250.00m, 1, 1500, (byte)16, new TimeOnly(5, 30, 0) },
                    { 2, "10 km Road Race", 10.00m, 150.00m, 1, 2000, (byte)12, new TimeOnly(6, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "DistanceKm", "EntryFee", "EventId", "MaxParticipants", "StartTime" },
                values: new object[] { 3, "5 km Fun Walk", 5.00m, 60.00m, 1, 1000, new TimeOnly(6, 30, 0) });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "DistanceKm", "EntryFee", "EventId", "MaxParticipants", "MinimumAge", "StartTime" },
                values: new object[,]
                {
                    { 4, "22 km Trail Run", 22.00m, 380.00m, 2, 300, (byte)18, new TimeOnly(6, 30, 0) },
                    { 5, "12 km Trail Run", 12.00m, 280.00m, 2, 400, (byte)14, new TimeOnly(7, 0, 0) },
                    { 6, "105 km Road Race", 105.00m, 650.00m, 3, 1200, (byte)16, new TimeOnly(6, 0, 0) },
                    { 7, "50 km Fun Ride", 50.00m, 350.00m, 3, 800, (byte)12, new TimeOnly(7, 30, 0) }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "DistanceKm", "EntryFee", "EventId", "MaxParticipants", "StartTime" },
                values: new object[] { 8, "8 km Heritage Walk", 8.00m, 80.00m, 4, 1500, new TimeOnly(8, 0, 0) });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "DistanceKm", "EventId", "MaxParticipants", "StartTime" },
                values: new object[] { 9, "3 km Family Walk", 3.00m, 4, 500, new TimeOnly(8, 30, 0) });

            migrationBuilder.InsertData(
                table: "Enrolments",
                columns: new[] { "EnrolmentId", "CategoryId", "EnrolledAt", "ParticipantId", "RaceNumber", "Status" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 6, 2, 9, 15, 0, 0, DateTimeKind.Unspecified), 3, "DSH1001", "Confirmed" },
                    { 2, 1, new DateTime(2026, 6, 10, 18, 42, 0, 0, DateTimeKind.Unspecified), 4, "DSH1002", "Confirmed" },
                    { 3, 1, new DateTime(2026, 7, 1, 7, 5, 0, 0, DateTimeKind.Unspecified), 8, "DSH1003", "Confirmed" },
                    { 4, 2, new DateTime(2026, 6, 15, 12, 30, 0, 0, DateTimeKind.Unspecified), 5, "DSH2001", "Confirmed" },
                    { 5, 2, new DateTime(2026, 7, 20, 20, 11, 0, 0, DateTimeKind.Unspecified), 7, "DSH2002", "Confirmed" },
                    { 6, 3, new DateTime(2026, 8, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), 6, "DSH3001", "Confirmed" },
                    { 7, 4, new DateTime(2026, 8, 20, 8, 25, 0, 0, DateTimeKind.Unspecified), 6, "SMT0101", "Confirmed" },
                    { 8, 5, new DateTime(2026, 9, 1, 19, 40, 0, 0, DateTimeKind.Unspecified), 3, "SMT0201", "Confirmed" },
                    { 9, 5, new DateTime(2026, 9, 5, 13, 5, 0, 0, DateTimeKind.Unspecified), 4, "SMT0202", "Cancelled" },
                    { 10, 6, new DateTime(2026, 9, 10, 6, 50, 0, 0, DateTimeKind.Unspecified), 5, "WCC0001", "Confirmed" },
                    { 11, 7, new DateTime(2026, 9, 12, 21, 15, 0, 0, DateTimeKind.Unspecified), 8, "WCC5001", "Confirmed" },
                    { 12, 7, new DateTime(2026, 9, 15, 16, 30, 0, 0, DateTimeKind.Unspecified), 7, null, "Pending" }
                });

            migrationBuilder.InsertData(
                table: "Results",
                columns: new[] { "ResultId", "CategoryPosition", "EnrolmentId", "FinishStatus", "FinishTime", "RecordedAt" },
                values: new object[,]
                {
                    { 1, 1, 1, "Finished", new TimeOnly(1, 38, 42), new DateTime(2026, 8, 16, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, 2, "Finished", new TimeOnly(1, 52, 10), new DateTime(2026, 8, 16, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, null, 3, "DNF", null, new DateTime(2026, 8, 16, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 1, 4, "Finished", new TimeOnly(0, 44, 5), new DateTime(2026, 8, 16, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, 2, 5, "Finished", new TimeOnly(0, 51, 30), new DateTime(2026, 8, 16, 13, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, 1, 6, "Finished", new TimeOnly(0, 38, 20), new DateTime(2026, 8, 16, 13, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_EventId_CategoryName",
                table: "Categories",
                columns: new[] { "EventId", "CategoryName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_CategoryId",
                table: "Enrolments",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_ParticipantId_CategoryId",
                table: "Enrolments",
                columns: new[] { "ParticipantId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventName_EventDate",
                table: "Events",
                columns: new[] { "EventName", "EventDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTypeId",
                table: "Events",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganiserId",
                table: "Events",
                column: "OrganiserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                table: "Events",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_TypeName",
                table: "EventTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_EnrolmentId",
                table: "Results",
                column: "EnrolmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_VenueName_City",
                table: "Venues",
                columns: new[] { "VenueName", "City" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Enrolments");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Venues");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
