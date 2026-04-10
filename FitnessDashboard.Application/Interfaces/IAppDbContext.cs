using FitnessDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessDashboard.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Athlete> Athletes { get; }
    DbSet<Activity> Activities { get; }
    DbSet<Gear> Gears { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
