using FitnessDashboard.Application.Interfaces;

namespace FitnessDashboard.Infrastructure.Weather;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public Task<(string Condition, double Multiplier)> GetWearFactorAsync(DateTime date, double? latitude = null, double? longitude = null)
    {
        // TODO 
        var condition = "Rain";
        var multiplier = 1.2;
        
        return Task.FromResult((condition, multiplier));
    }
}