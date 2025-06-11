# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/GloboClima.Web/GloboClima.Web.csproj", "src/GloboClima.Web/"]
COPY ["src/GloboClima.Application/GloboClima.Application.csproj", "src/GloboClima.Application/"]
COPY ["src/GloboClima.Domain/GloboClima.Domain.csproj", "src/GloboClima.Domain/"]

RUN dotnet restore "src/GloboClima.Web/GloboClima.Web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/GloboClima.Web"
RUN dotnet build "GloboClima.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GloboClima.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

# Environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GloboClima.Web.dll"]