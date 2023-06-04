using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeamCubing.DAL.Models;

namespace TeamCubing.DAL.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Room>().HasIndex(r => r.Name).IsUnique();

        builder.Entity<Room>()
            .HasMany(r => r.Users)
            .WithOne(u => u.Room)
            .HasForeignKey(u => u.RoomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Entity<Room>()
            .HasMany(r => r.Solves)
            .WithOne(s => s.Room)
            .HasForeignKey(s => s.RoomId);

        builder
            .Entity<RoomSolve>()
            .HasMany(s => s.Results)
            .WithOne(r => r.RoomSolve)
            .HasForeignKey(r => r.RoomSolveId);

        builder
            .Entity<RoomSolveResult>()
            .HasOne(r => r.User)
            .WithMany(u => u.RoomSolvesResults)
            .HasForeignKey(r => r.UserId);

        builder
            .Entity<ApplicationUser>()
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId);

        builder
            .Entity<Session>()
            .HasMany(s => s.Solves)
            .WithOne(s => s.Session)
            .HasForeignKey(s => s.SessionId);
    }
}
