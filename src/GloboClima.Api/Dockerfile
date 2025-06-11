# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/GloboClima.Api/GloboClima.Api.csproj", "src/GloboClima.Api/"]
COPY ["src/GloboClima.Application/GloboClima.Application.csproj", "src/GloboClima.Application/"]
COPY ["src/GloboClima.Domain/GloboClima.Domain.csproj", "src/GloboClima.Domain/"]
COPY ["src/GloboClima.Infrastructure/GloboClima.Infrastructure.csproj", "src/GloboClima.Infrastructure/"]

RUN dotnet restore "src/GloboClima.Api/GloboClima.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/GloboClima.Api"
RUN dotnet build "GloboClima.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GloboClima.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .

# Create non-root user
RUN useradd -m -u 1001 appuser && chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "GloboClima.Api.dll"]