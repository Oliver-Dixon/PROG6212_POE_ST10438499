using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RaceDay.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Database: SQL Server when the API runs, a fresh in-memory database when the unit tests run
if (builder.Environment.IsEnvironment("Testing"))
{
    // Each test starts the API again, so EF Core's "many service providers" warning is expected here
    var testDatabaseName = "RaceDayTests-" + Guid.NewGuid();
    builder.Services.AddDbContext<RaceDayContext>(options => options
        .UseInMemoryDatabase(testDatabaseName)
        .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning)));
}
else
{
    builder.Services.AddDbContext<RaceDayContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayDB")));
}

// Server-side session: after login it stores the user's ID and role for every later request
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers();

// Swagger: lists every endpoint with the descriptions from the /// comments in the controllers
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Code-First: create the database and apply the migrations (and seed data) on start-up
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseSession();
app.MapControllers();

app.Run();

// Lets the unit test project start the API
public partial class Program
{
}
