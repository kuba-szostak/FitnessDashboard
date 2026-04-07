using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

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
    
    public async Task<(string Condition, double Multiplier)> GetWearFactorAsync(DateTime date, double? latitude = null, double? longitude = null)
    {
        if (!latitude.HasValue || !longitude.HasValue)
        {
            return ("Unknown", 1.0);
        }

        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            return ("NoApiKey", 1.0);
        }

        long unixTime = ((DateTimeOffset)date).ToUnixTimeSeconds();
        
        var url = $"data/3.0/onecall/timemachine?lat={latitude}&lon={longitude}&dt={unixTime}&appid={_settings.ApiKey}&units=metric";        

        try
        {
            var response = await _httpClient.GetFromJsonAsync<WeatherResponse>(url);
            var weather = response?.Data?.FirstOrDefault()?.Weather?.FirstOrDefault();

            if (weather == null)
            {
                return ("Unknown", 1.0);
            }

            var condition = weather.Main;
            var multiplier = GetMultiplierForCondition(condition);

            return (condition, multiplier);
        }
        catch (Exception)
        {
            return ("Error", 1.0);
        }
    }

    private double GetMultiplierForCondition(string condition)
    {
        return condition.ToLower() switch
        {
            "thunderstorm" => 2.0,
            "drizzle" => 1.2,
            "rain" => 1.5,
            "snow" => 2.5,
            "mist" or "smoke" or "haze" or "dust" or "fog" or "sand" or "ash" or "squall" or "tornado" => 1.3,
            "clear" => 1.0,
            "clouds" => 1.0,
            _ => 1.0
        };
    }
}

public record WeatherResponse(
    [property: JsonPropertyName("data")] List<WeatherData> Data
);

public record WeatherData(
    [property: JsonPropertyName("weather")] List<WeatherCondition> Weather
);

public record WeatherCondition(
    [property: JsonPropertyName("main")] string Main,
    [property: JsonPropertyName("description")] string Description
);