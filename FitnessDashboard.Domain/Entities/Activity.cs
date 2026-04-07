namespace FitnessDashboard.Domain.Entities;

public class Activity
{
    public long Id { get; set; } // Strava Activity ID
    public long AthleteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Distance { get; set; } // in meters
    public int MovingTime { get; set; } // in seconds
    public string Type { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string? GearId { get; set; } // Strava Gear ID
    public double? StartLatitude { get; set; }
    public double? StartLongitude { get; set; }

    public Athlete Athlete { get; set; } = null!;
    public Gear? Gear { get; set; }
    
    public string? WeatherCondition { get; set; }
    
    public double WearMultiplier { get; set; } = 1.0;
    
}
