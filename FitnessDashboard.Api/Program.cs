using System;
using System.IO;
using FitnessDashboard.Application.Interfaces;
using FitnessDashboard.Application.Services;
using FitnessDashboard.Infrastructure.Configuration;
using FitnessDashboard.Infrastructure.Persistence;
using FitnessDashboard.Infrastructure.Strava;
using FitnessDashboard.Infrastructure.Sync;
using FitnessDashboard.Infrastructure.Weather;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load .env file for local development
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (!File.Exists(envPath))
{
    envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
}

if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            
            if (key == "STRAVA_CLIENT_ID") key = "Strava__ClientId";
            if (key == "STRAVA_CLIENT_SECRET") key = "Strava__ClientSecret";
            if (key == "WEATHER_API_KEY") key = "Weather__ApiKey";
            
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

// Add services to the container.
builder.Services.AddControllers();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<StravaSettings>(builder.Configuration.GetSection("Strava"));
builder.Services.Configure<WeatherSettings>(builder.Configuration.GetSection("Weather"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<IStravaService, StravaService>();
builder.Services.AddScoped<IGearMaintenanceService, GearMaintenanceService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddHostedService<StravaSyncWorker>();
builder.Services.AddHttpClient<IStravaService, StravaService>(client =>
{
    client.BaseAddress = new Uri("https://www.strava.com/api/v3/");
});

builder.Services.AddHttpClient<IWeatherService, WeatherService>(client => 
{
    client.BaseAddress = new Uri("https://api.openweathermap.org/data/3.0/");
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatically apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

