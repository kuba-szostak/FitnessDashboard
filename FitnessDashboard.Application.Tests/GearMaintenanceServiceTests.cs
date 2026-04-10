using FitnessDashboard.Application.Services;
using FitnessDashboard.Domain.Entities;
using Moq;
using FitnessDashboard.Application.Interfaces;
using Xunit;

namespace FitnessDashboard.Application.Tests;

public class GearMaintenanceServiceTests
{
    private readonly Mock<IAppDbContext> _mockContext;
    private readonly GearMaintenanceService _service;

    public GearMaintenanceServiceTests()
    {
        _mockContext = new Mock<IAppDbContext>();
        _service = new GearMaintenanceService(_mockContext.Object);
    }

    [Theory]
    [InlineData(0, MaintenanceStatus.Healthy)]
    [InlineData(500000, MaintenanceStatus.Healthy)] // 500km
    [InlineData(700000, MaintenanceStatus.Healthy)] // 700km - threshold is > 700
    [InlineData(700001, MaintenanceStatus.ServiceRequired)] // Just above 700km
    [InlineData(850000, MaintenanceStatus.ServiceRequired)] // 850km
    [InlineData(1000000, MaintenanceStatus.ServiceRequired)] // 1000km - threshold is > 1000
    [InlineData(1000001, MaintenanceStatus.Critical)] // Just above 1000km
    [InlineData(1500000, MaintenanceStatus.Critical)] // 1500km
    public void CalculateStatus_ShouldReturnCorrectStatus_BasedOnDistance(double distanceInMeters, MaintenanceStatus expectedStatus)
    {
        // Act
        var result = _service.CalculateStatus(distanceInMeters);

        // Assert
        Assert.Equal(expectedStatus, result);
    }
}
