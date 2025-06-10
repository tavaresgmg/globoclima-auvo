#!/bin/bash

echo "Building GloboClima Solution..."
echo "=============================="

# Build solution
dotnet build GloboClima.sln

if [ $? -eq 0 ]; then
    echo ""
    echo "Build completed successfully!"
    echo ""
    echo "You can now run the application using:"
    echo "  ./run-all.sh   - Run both API and Web"
    echo "  ./run-api.sh   - Run only the API"
    echo "  ./run-web.sh   - Run only the Web"
else
    echo ""
    echo "Build failed! Please check the errors above."
    exit 1
fi