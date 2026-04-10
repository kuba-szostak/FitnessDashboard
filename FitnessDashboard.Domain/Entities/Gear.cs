namespace FitnessDashboard.Domain.Entities;

public class Gear
{
    public string Id { get; set; } = string.Empty; // Strava Gear ID
    public long AthleteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double TotalDistance { get; set; } // in meters
    public bool IsPrimary { get; set; }
    
    public Athlete Athlete { get; set; } = null!;
    
    public MaintenanceStatus Status { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

public enum MaintenanceStatus
{
    Healthy,
    ServiceRequired,
    Critical
}

