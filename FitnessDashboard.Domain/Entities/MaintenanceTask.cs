namespace FitnessDashboard.Domain.Entities;

public class MaintenanceTask
{
    public Guid Id { get; set; }
    public string GearId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double IntervalMeters { get; set; }
    public double LastServiceMeters { get; set; }
    public MaintenanceStatus Status { get; set; }
    
    public Gear Gear { get; set; } = null!;
}

public enum MaintenanceStatus
{
    Healthy,
    ServiceRequired,
    Critical
}
