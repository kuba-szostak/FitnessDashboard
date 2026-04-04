namespace FitnessDashboard.Infrastructure.Configuration;

public class StravaSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class WeatherSettings
{
    public string ApiKey { get; set; } = string.Empty;
}
