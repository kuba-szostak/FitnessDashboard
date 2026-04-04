using FitnessDashboard.Domain.Entities;

namespace FitnessDashboard.Application.Interfaces;

public interface IStravaService
{
    Task<IEnumerable<Activity>> GetActivitiesAsync(Athlete athlete, DateTime? after = null);
    Task<IEnumerable<Gear>> GetAthleteGearsAsync(Athlete athlete);
    Task<Athlete> RefreshTokenAsync(Athlete athlete);
}
