using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace FitnessDashboard.Infrastructure.Weather;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherSettings _settings;

    public WeatherService(HttpClient httpClient, IOptions<WeatherSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }
    
    public Task<(string Condition, double Multiplier)> GetWearFactorAsync(DateTime date, double? latitude = null, double? longitude = null)
    {
        // TODO 
        var condition = "Rain";
        var multiplier = 1.2;
        
        return Task.FromResult((condition, multiplier));
    }
}