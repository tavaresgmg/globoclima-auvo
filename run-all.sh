#!/bin/bash

echo "Starting GloboClima Application..."
echo "================================"

# Start API in background
echo "Starting API on https://localhost:7219 and http://localhost:5057..."
cd src/GloboClima.Api
dotnet run --launch-profile https > ../../api.log 2>&1 &
API_PID=$!

# Wait a bit for API to start
sleep 5

# Start Web in background
echo "Starting Web on http://localhost:5029..."
cd ../GloboClima.Web
dotnet run --launch-profile http > ../../web.log 2>&1 &
WEB_PID=$!

cd ../..

echo ""
echo "Services started:"
echo "- API: https://localhost:7219 (Swagger: https://localhost:7219/swagger)"
echo "- Web: http://localhost:5029"
echo ""
echo "Logs are being written to:"
echo "- API: api.log"
echo "- Web: web.log"
echo ""
echo "Press Ctrl+C to stop all services"

# Function to cleanup on exit
cleanup() {
    echo ""
    echo "Stopping services..."
    kill $API_PID 2>/dev/null
    kill $WEB_PID 2>/dev/null
    rm -f api.log web.log
    echo "Services stopped"
    exit 0
}

# Set trap to cleanup on Ctrl+C
trap cleanup INT

# Wait for processes
wait