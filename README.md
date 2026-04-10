# Fitness Gear Maintenance Dashboard

A Blazor WebAssembly application designed to help athletes track the maintenance status of their fitness gear (shoes, bikes, etc.) by synchronizing data from Strava and applying wear factors based on weather conditions that occured during the activity.

## Technologies
- **Frontend**: Blazor WebAssembly, MudBlazor UI Components.
- **Backend**: ASP.NET Core Web API, Entity Framework Core.
- **Database**: SQLite.
- **Containerization**: Docker & Docker Compose.
- **APIs**: Strava API v3, OpenWeatherMap API 3.0.

## Prerequisites
- [.NET 8 SDK]
- [Docker Desktop]
- Strava API Credentials (Client ID and Client Secret from [Strava My App](https://www.strava.com/settings/api))
- OpenWeatherMap API Key (from [OpenWeatherMap](https://openweathermap.org/api))

## Setup & Configuration

### 1. Environment Variables
Create a `.env` file in the project root with the following keys:
```env
STRAVA_CLIENT_ID=your_strava_client_id
STRAVA_CLIENT_SECRET=your_strava_client_secret
WEATHER_API_KEY=your_openweathermap_api_key
```

### 2. Running with Docker
The easiest way to get the dashboard up and running is using Docker Compose:
```powershell
docker-compose up --build -d
```
Access the application at `http://localhost:8080`.

## Usage
1. Navigate to the **Settings** page.
2. Click **Log in with Strava** to authorize the application.
3. Once authorized, the application will perform an initial sync of your gear and activities.
4. View your gear status on the **Home** page.
5. Background sync runs every 5 minutes to keep your data up to date.
