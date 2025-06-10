#!/bin/bash

echo "Starting GloboClima in Development Mode..."
echo "========================================="

# Function to open new terminal tab (macOS)
open_terminal_tab() {
    osascript -e "tell application \"Terminal\" to do script \"cd $(pwd) && $1\""
}

# Check if running on macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS - open in new terminal tabs
    echo "Opening API in new terminal tab..."
    open_terminal_tab "./run-api.sh"
    
    sleep 2
    
    echo "Opening Web in new terminal tab..."
    open_terminal_tab "./run-web.sh"
    
    echo ""
    echo "Services are starting in separate terminal tabs:"
    echo "- API: https://localhost:7219 (Swagger: https://localhost:7219/swagger)"
    echo "- Web: http://localhost:5029"
    echo ""
    echo "Check the terminal tabs for logs"
else
    # Other OS - run in background with log tailing
    ./run-all.sh &
    MAIN_PID=$!
    
    sleep 5
    
    echo ""
    echo "Tailing logs (Ctrl+C to stop)..."
    echo "================================"
    
    # Tail both log files
    tail -f api.log web.log
    
    # Kill main process on exit
    kill $MAIN_PID 2>/dev/null
fi