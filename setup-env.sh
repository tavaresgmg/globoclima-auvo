#!/bin/bash

echo "GloboClima Environment Setup"
echo "==========================="
echo ""

# Check if OpenWeatherMap API key is configured
if grep -q "YOUR_OPENWEATHERMAP_API_KEY" src/GloboClima.Api/appsettings.json; then
    echo "⚠️  OpenWeatherMap API key not configured!"
    echo ""
    echo "To configure:"
    echo "1. Get your API key from https://openweathermap.org/api"
    echo "2. Edit src/GloboClima.Api/appsettings.json"
    echo "3. Replace YOUR_OPENWEATHERMAP_API_KEY with your actual key"
    echo ""
fi

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found!"
    echo "Please install .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
else
    echo "✓ .NET SDK installed: $(dotnet --version)"
fi

# Check if AWS CLI is configured (optional for local development)
if command -v aws &> /dev/null; then
    if aws sts get-caller-identity &> /dev/null; then
        echo "✓ AWS CLI configured"
    else
        echo "⚠️  AWS CLI not configured (optional for local development)"
    fi
else
    echo "⚠️  AWS CLI not installed (optional for local development)"
fi

echo ""
echo "Environment check complete!"
echo ""
echo "Next steps:"
echo "1. Configure OpenWeatherMap API key (if not done)"
echo "2. Run: ./build-all.sh"
echo "3. Run: ./run-all.sh"