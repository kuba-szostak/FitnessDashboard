using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Domain.Entities;
using FitnessDashboard.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FitnessDashboard.Infrastructure.Strava;

public class StravaService : IStravaService
{
    private readonly HttpClient _httpClient;
    private readonly StravaSettings _settings;

    public StravaService(HttpClient httpClient, IOptions<StravaSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<IEnumerable<Activity>> GetActivitiesAsync(Athlete athlete, DateTime? after = null)
    {
        await RefreshTokenAsync(athlete);

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
        await RefreshTokenAsync(athlete);
        
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", athlete.AccessToken);
        
        // Strava API: GET https://www.strava.com/api/v3/athlete
        // The athlete profile contains a list of bikes and shoes
        var response = await _httpClient.GetFromJsonAsync<StravaAthleteProfileResponse>("athlete");
        
        var gears = new List<Gear>();
        
        if (response?.Bikes != null)
        {
            gears.AddRange(response.Bikes.Select(b => new Gear
            {
                Id = b.Id,
                AthleteId = athlete.Id,
                Name = b.Name,
                TotalDistance = b.Distance,
                IsPrimary = b.Primary
            }));
        }
        
        if (response?.Shoes != null)
        {
            gears.AddRange(response.Shoes.Select(s => new Gear
            {
                Id = s.Id,
                AthleteId = athlete.Id,
                Name = s.Name,
                TotalDistance = s.Distance,
                IsPrimary = s.Primary
            }));
        }
        
        return gears;
    }

    public async Task<Athlete> RefreshTokenAsync(Athlete athlete)
    {
        // Strava OAuth token refresh: POST https://www.strava.com/oauth/token
        // Params: client_id, client_secret, refresh_token, grant_type=refresh_token
        
        if (string.IsNullOrEmpty(_settings.ClientId) || string.IsNullOrEmpty(_settings.ClientSecret))
        {
            return athlete;
        }

        // Only refresh if it's expired or about to expire (within 5 minutes)
        if (athlete.TokenExpiresAt > DateTime.UtcNow.AddMinutes(5))
        {
            return athlete;
        }

        var dict = new Dictionary<string, string>
        {
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret },
            { "refresh_token", athlete.RefreshToken },
            { "grant_type", "refresh_token" }
        };

        var response = await _httpClient.PostAsync("https://www.strava.com/oauth/token", new FormUrlEncodedContent(dict));

        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = await response.Content.ReadFromJsonAsync<StravaTokenResponse>();
            if (tokenResponse != null)
            {
                athlete.AccessToken = tokenResponse.AccessToken;
                athlete.RefreshToken = tokenResponse.RefreshToken;
                athlete.TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            }
        }

        return athlete;
    }

    public async Task<Athlete> ExchangeCodeAsync(string code)
    {
        var dict = new Dictionary<string, string>
        {
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret },
            { "code", code },
            { "grant_type", "authorization_code" }
        };

        var response = await _httpClient.PostAsync("https://www.strava.com/oauth/token", new FormUrlEncodedContent(dict));
        response.EnsureSuccessStatusCode();

        var exchangeResponse = await response.Content.ReadFromJsonAsync<StravaExchangeResponse>();
        if (exchangeResponse == null) throw new Exception("Failed to exchange code");

        return new Athlete
        {
            Id = exchangeResponse.Athlete.Id,
            FirstName = exchangeResponse.Athlete.FirstName,
            LastName = exchangeResponse.Athlete.LastName,
            ProfileImageUrl = exchangeResponse.Athlete.Profile,
            AccessToken = exchangeResponse.AccessToken,
            RefreshToken = exchangeResponse.RefreshToken,
            TokenExpiresAt = DateTime.UtcNow.AddSeconds(exchangeResponse.ExpiresIn)
        };
    }
}

public record StravaExchangeResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("athlete")] StravaAthleteSummary Athlete
);

public record StravaAthleteSummary(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("firstname")] string FirstName,
    [property: JsonPropertyName("lastname")] string LastName,
    [property: JsonPropertyName("profile")] string Profile
);

public record StravaTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);

public record StravaAthleteProfileResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("bikes")] List<StravaGearResponse>? Bikes,
    [property: JsonPropertyName("shoes")] List<StravaGearResponse>? Shoes
);

public record StravaGearResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("distance")] double Distance,
    [property: JsonPropertyName("primary")] bool Primary
);

public record StravaActivityResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("distance")] double Distance,
    [property: JsonPropertyName("moving_time")] int MovingTime,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("start_date")] DateTime StartDate,
    [property: JsonPropertyName("gear_id")] string? GearId
);
