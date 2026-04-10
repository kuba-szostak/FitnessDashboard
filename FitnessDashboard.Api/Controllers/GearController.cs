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
            .Select(g => new GearDto(
                g.Id,
                g.Name,
                g.TotalDistance,
                g.IsPrimary,
                g.Status.ToString()
            ))
            .ToListAsync();

        return Ok(gears);
    }
}
