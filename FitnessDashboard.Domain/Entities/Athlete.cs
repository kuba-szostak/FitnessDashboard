namespace FitnessDashboard.Domain.Entities;

public class Athlete
{
    public long Id { get; set; } // Strava Athlete ID
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<Gear> Gears { get; set; } = new List<Gear>();
}
