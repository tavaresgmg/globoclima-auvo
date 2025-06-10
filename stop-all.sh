#!/bin/bash

echo "Stopping GloboClima Services..."
echo "=============================="

# Find and kill dotnet processes running GloboClima
API_PIDS=$(ps aux | grep "dotnet.*GloboClima.Api" | grep -v grep | awk '{print $2}')
WEB_PIDS=$(ps aux | grep "dotnet.*GloboClima.Web" | grep -v grep | awk '{print $2}')

if [ ! -z "$API_PIDS" ]; then
    echo "Stopping API processes: $API_PIDS"
    kill $API_PIDS 2>/dev/null
else
    echo "API is not running"
fi

if [ ! -z "$WEB_PIDS" ]; then
    echo "Stopping Web processes: $WEB_PIDS"
    kill $WEB_PIDS 2>/dev/null
else
    echo "Web is not running"
fi

# Clean up log files
if [ -f "api.log" ] || [ -f "web.log" ]; then
    echo "Cleaning up log files..."
    rm -f api.log web.log
fi

echo ""
echo "All services stopped"