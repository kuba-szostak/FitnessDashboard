namespace FitnessDashboard.Application.Interfaces;

public interface IWeatherService
{
    Task<(string Condition, double Multiplier)> GetWearFactorAsync(DateTime date, double? latitude = null,
        double? longitude = null);
}