using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Application.Services;
using FitnessDashboard.Domain.Entities;
using FitnessDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FitnessDashboard.Application.Tests;

public class SyncServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IStravaService> _mockStravaService;
    private readonly Mock<IWeatherService> _mockWeatherService;
    private readonly Mock<IGearMaintenanceService> _mockMaintenanceService;
    private readonly Mock<ILogger<SyncService>> _mockLogger;
    private readonly SyncService _syncService;

    public SyncServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _mockStravaService = new Mock<IStravaService>();
        _mockWeatherService = new Mock<IWeatherService>();
        _mockMaintenanceService = new Mock<IGearMaintenanceService>();
        _mockLogger = new Mock<ILogger<SyncService>>();

        _syncService = new SyncService(
            _context,
            _mockStravaService.Object,
            _mockWeatherService.Object,
            _mockMaintenanceService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SyncAthleteDataAsync_ShouldAddMissingGear()
    {
        // Arrange
        var athlete = new Athlete { Id = 1, FirstName = "John", LastName = "Doe" };
        _context.Athletes.Add(athlete);
        await _context.SaveChangesAsync();

        var stravaGears = new List<Gear>
        {
            new Gear { Id = "g1", Name = "Bike 1", AthleteId = 1 }
        };

        _mockStravaService.Setup(s => s.GetAthleteGearsAsync(athlete))
            .ReturnsAsync(stravaGears);
        _mockStravaService.Setup(s => s.GetActivitiesAsync(athlete, It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<Activity>());

        // Act
        await _syncService.SyncAthleteDataAsync(athlete);

        // Assert
        var gear = await _context.Gears.FirstOrDefaultAsync(g => g.Id == "g1");
        Assert.NotNull(gear);
        Assert.Equal("Bike 1", gear.Name);
    }

    [Fact]
    public async Task SyncAthleteDataAsync_ShouldAddActivitiesAndApplyWeatherFactor()
    {
        // Arrange
        var athlete = new Athlete { Id = 1, FirstName = "John", LastName = "Doe" };
        var gear = new Gear { Id = "g1", Name = "Bike 1", AthleteId = 1, TotalDistance = 0 };
        _context.Athletes.Add(athlete);
        _context.Gears.Add(gear);
        await _context.SaveChangesAsync();

        var stravaActivities = new List<Activity>
        {
            new Activity 
            { 
                Id = 101, 
                Name = "Ride 1", 
                Distance = 1000, 
                AthleteId = 1, 
                GearId = "g1", 
                StartDate = DateTime.UtcNow 
            }
        };

        _mockStravaService.Setup(s => s.GetAthleteGearsAsync(athlete))
            .ReturnsAsync(new List<Gear> { gear });
        _mockStravaService.Setup(s => s.GetActivitiesAsync(athlete, It.IsAny<DateTime?>()))
            .ReturnsAsync(stravaActivities);
        _mockWeatherService.Setup(w => w.GetWearFactorAsync(It.IsAny<DateTime>(), It.IsAny<double?>(), It.IsAny<double?>()))
            .ReturnsAsync(("Rainy", 1.5));

        // Act
        await _syncService.SyncAthleteDataAsync(athlete);

        // Assert
        var activity = await _context.Activities.FindAsync(101L);
        Assert.NotNull(activity);
        Assert.Equal("Rainy", activity.WeatherCondition);
        Assert.Equal(1.5, activity.WearMultiplier);

        var updatedGear = await _context.Gears.FindAsync("g1");
        Assert.Equal(1500, updatedGear!.TotalDistance); // 1000 * 1.5
        
        _mockMaintenanceService.Verify(m => m.UpdateGearMaintenanceStatusAsync("g1"), Times.Once);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
