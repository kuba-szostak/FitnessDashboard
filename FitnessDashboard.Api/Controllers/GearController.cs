using FitnessDashboard.Infrastructure.Persistence;
using FitnessDashboard.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GearController : ControllerBase
{
    private readonly AppDbContext _context;

    public GearController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GearDto>>> GetGears()
    {
        var gears = await _context.Gears
            .Include(g => g.MaintenanceTasks)
            .Select(g => new GearDto(
                g.Id,
                g.Name,
                g.TotalDistance,
                g.IsPrimary,
                g.MaintenanceTasks.Select(t => new MaintenanceTaskDto(
                    t.Id,
                    t.GearId,
                    t.Description,
                    t.IntervalMeters,
                    t.LastServiceMeters,
                    t.Status.ToString()
                )).ToList()
            ))
            .ToListAsync();

        return Ok(gears);
    }

    [HttpPost("{gearId}/maintenance")]
    public async Task<IActionResult> AddMaintenanceTask(string gearId, MaintenanceTaskDto taskDto)
    {
        var gear = await _context.Gears.FindAsync(gearId);
        if (gear == null) return NotFound();

        var task = new Domain.Entities.MaintenanceTask
        {
            Id = Guid.NewGuid(),
            GearId = gearId,
            Description = taskDto.Description,
            IntervalMeters = taskDto.IntervalMeters,
            LastServiceMeters = gear.TotalDistance,
            Status = Domain.Entities.MaintenanceStatus.Healthy
        };

        _context.MaintenanceTasks.Add(task);
        await _context.SaveChangesAsync();

        return Ok();
    }
}
