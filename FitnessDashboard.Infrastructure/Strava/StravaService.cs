using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FitnessDashboard.Infrastructure.Strava;

public class StravaService : IStravaService
{
    private readonly HttpClient _httpClient;

    public StravaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Activity>> GetActivitiesAsync(Athlete athlete, DateTime? after = null)
    {
        var url = "athlete/activities";
        if (after.HasValue)
        {
            var unixTimestamp = ((DateTimeOffset)after.Value).ToUnixTimeSeconds();
            url += $"?after={unixTimestamp}";
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", athlete.AccessToken);
        
        var response = await _httpClient.GetFromJsonAsync<List<StravaActivityResponse>>(url);
        
        return response?.Select(r => new Activity
        {
            Id = r.Id,
            AthleteId = athlete.Id,
            Name = r.Name,
            Distance = r.Distance,
            MovingTime = r.MovingTime,
            Type = r.Type,
            StartDate = r.StartDate,
            GearId = r.GearId
        }) ?? Enumerable.Empty<Activity>();
    }

    public async Task<IEnumerable<Gear>> GetAthleteGearsAsync(Athlete athlete)
    {
        // Strava API doesn't have a simple "get all gears" endpoint, 
        // usually gears are fetched via Athlete profile or individual gear ID.
        // Simplified for this prototype.
        return Enumerable.Empty<Gear>();
    }

    public async Task<Athlete> RefreshTokenAsync(Athlete athlete)
    {
        // Implementation of OAuth token refresh would go here
        return athlete;
    }
}

public record StravaActivityResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("distance")] double Distance,
    [property: JsonPropertyName("moving_time")] int MovingTime,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("start_date")] DateTime StartDate,
    [property: JsonPropertyName("gear_id")] string? GearId
);
