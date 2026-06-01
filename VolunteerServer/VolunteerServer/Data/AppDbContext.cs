using Microsoft.EntityFrameworkCore;
using VolunteerServer.Models;

namespace VolunteerServer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventSlot> EventSlots { get; set; }
    public DbSet<SlotVolunteer> SlotVolunteers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<SlotVolunteer>()
            .HasIndex(sv => new { sv.UserId, sv.SlotId })
            .HasFilter("\"status\" = 'registered'")
            .IsUnique();

        // Внешние ключи (без навигационных свойств)
        modelBuilder.Entity<Event>()
            .HasIndex(e => e.CreatedBy);

        modelBuilder.Entity<EventSlot>()
            .HasIndex(es => es.EventId);

        modelBuilder.Entity<SlotVolunteer>()
            .HasIndex(sv => sv.UserId);

        modelBuilder.Entity<SlotVolunteer>()
            .HasIndex(sv => sv.SlotId);
    }
}