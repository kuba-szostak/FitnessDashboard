using FitnessDashboard.Domain.Entities;

namespace FitnessDashboard.Application.Interfaces;

public interface IGearMaintenanceService
{
    Task UpdateGearMaintenanceStatusAsync(string gearId);
    MaintenanceStatus CalculateStatus(double totalDistance);
}
