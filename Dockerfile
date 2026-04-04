# Dockerfile dla FitnessDashboard (Clean Architecture)

# Build Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-backend
WORKDIR /src
COPY ["FitnessDashboard.Api/FitnessDashboard.Api.csproj", "FitnessDashboard.Api/"]
COPY ["FitnessDashboard.Application/FitnessDashboard.Application.csproj", "FitnessDashboard.Application/"]
COPY ["FitnessDashboard.Domain/FitnessDashboard.Domain.csproj", "FitnessDashboard.Domain/"]
COPY ["FitnessDashboard.Infrastructure/FitnessDashboard.Infrastructure.csproj", "FitnessDashboard.Infrastructure/"]
COPY ["FitnessDashboard.Shared/FitnessDashboard.Shared.csproj", "FitnessDashboard.Shared/"]

RUN dotnet restore "FitnessDashboard.Api/FitnessDashboard.Api.csproj"
COPY . .
WORKDIR "/src/FitnessDashboard.Api"
RUN dotnet build "FitnessDashboard.Api.csproj" -c Release -o /app/build
RUN dotnet publish "FitnessDashboard.Api.csproj" -c Release -o /app/publish

# Build Frontend (Blazor)
FROM build-backend AS build-frontend
WORKDIR /src/FitnessDashboard.Client
RUN dotnet build "FitnessDashboard.Client.csproj" -c Release -o /app/build-client
RUN dotnet publish "FitnessDashboard.Client.csproj" -c Release -o /app/publish-client

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN mkdir /app/data
COPY --from=build-backend /app/publish .
# W rzeczywistym scenariuszu Blazor WASM może być serwowany jako static files z API lub oddzielnego Nginx
COPY --from=build-frontend /app/publish-client/wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "FitnessDashboard.Api.dll"]
