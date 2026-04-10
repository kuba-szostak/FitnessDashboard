using FitnessDashboard.Infrastructure.Persistence;
using FitnessDashboard.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace FitnessDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AthleteController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IStravaService _stravaService;
    private readonly ISyncService _syncService;
    private readonly StravaSettings _settings;

    public AthleteController(AppDbContext context, IStravaService stravaService, ISyncService syncService, IOptions<StravaSettings> settings)
    {
        _context = context;
        _stravaService = stravaService;
        _syncService = syncService;
        _settings = settings.Value;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Athlete>>> GetAthletes()
    {
        return await _context.Athletes.ToListAsync();
    }

    [HttpGet("auth-url")]
    public IActionResult GetAuthUrl([FromQuery] string redirectUri)
    {
        var url = $"https://www.strava.com/oauth/authorize?client_id={_settings.ClientId}&redirect_uri={redirectUri}&response_type=code&scope=read,activity:read_all,profile:read_all";
        return Ok(new { Url = url });
    }

    [HttpPost("exchange-code")]
    public async Task<IActionResult> ExchangeCode([FromBody] string code)
    {
        try
        {
            var athlete = await _stravaService.ExchangeCodeAsync(code);
            var existing = await _context.Athletes.FindAsync(athlete.Id);
            if (existing != null)
            {
                existing.AccessToken = athlete.AccessToken;
                existing.RefreshToken = athlete.RefreshToken;
                existing.TokenExpiresAt = athlete.TokenExpiresAt;
                existing.FirstName = athlete.FirstName;
                existing.LastName = athlete.LastName;
                existing.ProfileImageUrl = athlete.ProfileImageUrl;
            }
            else
            {
                _context.Athletes.Add(athlete);
            }

            await _context.SaveChangesAsync();
            
            // Trigger immediate sync
            try
            {
                await _syncService.SyncAthleteDataAsync(athlete);
            }
            catch (Exception syncEx)
            {
                // Log sync error but don't fail the exchange process
                Console.WriteLine($"Initial sync failed: {syncEx.Message}");
            }

            return Ok(athlete);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
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
