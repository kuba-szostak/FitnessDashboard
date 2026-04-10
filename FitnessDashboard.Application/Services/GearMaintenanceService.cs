using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitnessDashboard.Application.Services;

public class GearMaintenanceService : IGearMaintenanceService
{
    private readonly IAppDbContext _context;

    public GearMaintenanceService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task UpdateGearMaintenanceStatusAsync(string gearId)
    {
        var gear = await _context.Gears
            .Include(g => g.Status)
            .FirstOrDefaultAsync(g => g.Id == gearId);

        if (gear == null) return;

        gear.Status = CalculateStatus(gear.TotalDistance);
        
        await _context.SaveChangesAsync();
    }

    public MaintenanceStatus CalculateStatus(double totalDistance)
    {
        switch (totalDistance / 1000.0)
        {
            case > 1000:
                return MaintenanceStatus.Critical;
            case > 700:
                return MaintenanceStatus.ServiceRequired;
            default:
                return MaintenanceStatus.Healthy;
        }
        
    }
}
