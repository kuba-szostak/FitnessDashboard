using FitnessDashboard.Domain.Entities;

namespace FitnessDashboard.Application.Interfaces;

public interface ISyncService
{
    Task SyncAthleteDataAsync(Athlete athlete, CancellationToken ct = default);
    Task SyncAllAthletesAsync(CancellationToken ct = default);
}
