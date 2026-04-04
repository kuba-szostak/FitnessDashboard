namespace FitnessDashboard.Shared.DTOs;

public record AthleteDto(
    long Id,
    string FirstName,
    string LastName,
    string? ProfileImageUrl
);

public record ActivityDto(
    long Id,
    string Name,
    double Distance,
    int MovingTime,
    string Type,
    DateTime StartDate,
    string? GearId
);

public record GearDto(
    string Id,
    string Name,
    double TotalDistance,
    bool IsPrimary,
    List<MaintenanceTaskDto> MaintenanceTasks
);

public record MaintenanceTaskDto(
    Guid Id,
    string GearId,
    string Description,
    double IntervalMeters,
    double LastServiceMeters,
    string Status
);
