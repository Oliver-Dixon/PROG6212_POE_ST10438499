using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Database: SQL Server, using the "RaceDayDB" connection string in appsettings.json
builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayDB")));

builder.Services.AddControllers();

// Swagger: lists every endpoint so it can be tested in the browser
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Code-First: create the database and apply the migrations (and seed data) on start-up
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
