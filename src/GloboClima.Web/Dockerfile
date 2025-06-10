# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/GloboClima.Web/GloboClima.Web.csproj", "src/GloboClima.Web/"]
COPY ["src/GloboClima.Domain/GloboClima.Domain.csproj", "src/GloboClima.Domain/"]

RUN dotnet restore "src/GloboClima.Web/GloboClima.Web.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/GloboClima.Web"
RUN dotnet build "GloboClima.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GloboClima.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENTRYPOINT ["dotnet", "GloboClima.Web.dll"]