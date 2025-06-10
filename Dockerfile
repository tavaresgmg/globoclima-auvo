# Multi-stage build for GloboClima API
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution and project files
COPY GloboClima.sln ./
COPY src/GloboClima.Api/GloboClima.Api.csproj ./src/GloboClima.Api/
COPY src/GloboClima.Application/GloboClima.Application.csproj ./src/GloboClima.Application/
COPY src/GloboClima.Domain/GloboClima.Domain.csproj ./src/GloboClima.Domain/
COPY src/GloboClima.Infrastructure/GloboClima.Infrastructure.csproj ./src/GloboClima.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ ./src/

# Build and publish
WORKDIR /src/src/GloboClima.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Install cultures (for globalization)
RUN apk add --no-cache icu-libs

# Create non-root user
RUN addgroup -g 1000 -S globoclima && \
    adduser -u 1000 -S globoclima -G globoclima

# Copy published app
COPY --from=build --chown=globoclima:globoclima /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Switch to non-root user
USER globoclima

# Start the application
ENTRYPOINT ["dotnet", "GloboClima.Api.dll"]