using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessDashboard.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Athlete> Athletes { get; set; } = null!;
    public DbSet<Activity> Activities { get; set; } = null!;
    public DbSet<Gear> Gears { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Athlete>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Gear>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(g => g.Athlete)
                  .WithMany(a => a.Gears)
                  .HasForeignKey(g => g.AthleteId);
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(a => a.Athlete)
                  .WithMany(ath => ath.Activities)
                  .HasForeignKey(a => a.AthleteId);
            entity.HasOne(a => a.Gear)
                  .WithMany(g => g.Activities)
                  .HasForeignKey(a => a.GearId);
        });
    }
}
