using FitnessDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessDashboard.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Athlete> Athletes { get; }
    DbSet<Activity> Activities { get; }
    DbSet<Gear> Gears { get; }
    DbSet<MaintenanceTask> MaintenanceTasks { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
