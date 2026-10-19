using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

// The Code-First database. Every table, key and constraint matches the Part 1 ERD and SQL script.
public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UNIQUE constraints
        modelBuilder.Entity<Role>().HasIndex(r => r.RoleName).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<EventType>().HasIndex(t => t.TypeName).IsUnique();
        modelBuilder.Entity<Venue>().HasIndex(v => new { v.VenueName, v.City }).IsUnique();
        modelBuilder.Entity<Event>().HasIndex(e => new { e.EventName, e.EventDate }).IsUnique();
        modelBuilder.Entity<Category>().HasIndex(c => new { c.EventId, c.CategoryName }).IsUnique();
        modelBuilder.Entity<Enrolment>().HasIndex(e => new { e.ParticipantId, e.CategoryId }).IsUnique();

        // DEFAULT values
        // HasSentinel(true) tells EF Core that "true" is the normal value, so SQL Server's DEFAULT 1 applies
        modelBuilder.Entity<User>().Property(u => u.IsActive).HasDefaultValue(true).HasSentinel(true);
        modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        modelBuilder.Entity<Event>().Property(e => e.Status).HasDefaultValue("Draft");
        modelBuilder.Entity<Event>().Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        modelBuilder.Entity<Category>().Property(c => c.EntryFee).HasDefaultValue(0m);
        modelBuilder.Entity<Category>().Property(c => c.MinimumAge).HasDefaultValue((byte)0);
        modelBuilder.Entity<Enrolment>().Property(e => e.Status).HasDefaultValue("Confirmed");
        modelBuilder.Entity<Enrolment>().Property(e => e.EnrolledAt).HasDefaultValueSql("SYSUTCDATETIME()");
        modelBuilder.Entity<Result>().Property(r => r.FinishStatus).HasDefaultValue("Finished");
        modelBuilder.Entity<Result>().Property(r => r.RecordedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Relationships (foreign keys). Restrict = NO ACTION on delete, like the SQL script.
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role).WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser).WithMany(u => u.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.EventType).WithMany(t => t.Events)
            .HasForeignKey(e => e.EventTypeId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Venue).WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event).WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant).WithMany(u => u.Enrolments)
            .HasForeignKey(e => e.ParticipantId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category).WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment).WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId).OnDelete(DeleteBehavior.Cascade);

        // CHECK constraints
        modelBuilder.Entity<Event>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Events_Status", "[Status] IN ('Draft', 'Open', 'Closed', 'Completed', 'Cancelled')");
            t.HasCheckConstraint("CK_Events_RegistrationDeadline", "[RegistrationDeadline] <= [EventDate]");
        });

        modelBuilder.Entity<Category>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Categories_DistanceKm", "[DistanceKm] > 0");
            t.HasCheckConstraint("CK_Categories_EntryFee", "[EntryFee] >= 0");
            t.HasCheckConstraint("CK_Categories_MaxParticipants", "[MaxParticipants] > 0");
        });

        modelBuilder.Entity<Enrolment>().ToTable(t =>
            t.HasCheckConstraint("CK_Enrolments_Status", "[Status] IN ('Pending', 'Confirmed', 'Cancelled')"));

        modelBuilder.Entity<Result>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Results_FinishStatus", "[FinishStatus] IN ('Finished', 'DNF', 'DNS', 'DSQ')");
            t.HasCheckConstraint("CK_Results_CategoryPosition", "[CategoryPosition] > 0");
            t.HasCheckConstraint("CK_Results_FinishTime_Position",
                "([FinishStatus] = 'Finished' AND [FinishTime] IS NOT NULL AND [CategoryPosition] IS NOT NULL) OR " +
                "([FinishStatus] <> 'Finished' AND [FinishTime] IS NULL AND [CategoryPosition] IS NULL)");
        });
    }
}
