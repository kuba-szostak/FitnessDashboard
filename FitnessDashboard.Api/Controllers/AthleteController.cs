using FitnessDashboard.Infrastructure.Persistence;
using FitnessDashboard.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessDashboard.Application.Interfaces;

namespace FitnessDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AthleteController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IStravaService _stravaService;

    public AthleteController(AppDbContext context, IStravaService stravaService)
    {
        _context = context;
        _stravaService = stravaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Athlete>>> GetAthletes()
    {
        return await _context.Athletes.ToListAsync();
    }

    [HttpPost("setup-test")]
    public async Task<IActionResult> SetupTestAthlete([FromBody] Athlete athlete)
    {
        var existing = await _context.Athletes.FindAsync(athlete.Id);
        if (existing != null)
        {
            existing.AccessToken = athlete.AccessToken;
            existing.RefreshToken = athlete.RefreshToken;
            existing.FirstName = athlete.FirstName;
            existing.LastName = athlete.LastName;
        }
        else
        {
            _context.Athletes.Add(athlete);
        }

        await _context.SaveChangesAsync();
        return Ok(athlete);
    }
}
