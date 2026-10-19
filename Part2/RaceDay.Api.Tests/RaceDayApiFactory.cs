using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RaceDay.Api.Tests;

// Starts the real API in memory for the tests.
// The "Testing" environment makes Program.cs use a fresh in-memory database with the Part 1 seed data.
public class RaceDayApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
