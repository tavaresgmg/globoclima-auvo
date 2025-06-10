#\!/bin/bash

echo "Checking GloboClima Services Status..."
echo "===================================="

# Check API
echo -n "API (https://localhost:7219): "
if curl -k -s https://localhost:7219/swagger/index.html > /dev/null 2>&1; then
    echo "✓ Running"
else
    echo "✗ Not running"
fi

# Check Web
echo -n "Web (http://localhost:5029): "
if curl -s http://localhost:5029 > /dev/null 2>&1; then
    echo "✓ Running"
else
    echo "✗ Not running"
fi

echo ""
echo "Checking API endpoints..."
echo "------------------------"

# Check if API responds
API_RESPONSE=$(curl -k -s -o /dev/null -w "%{http_code}" https://localhost:7219/api/countries)
echo "Countries endpoint: HTTP $API_RESPONSE"

echo ""
echo "Quick links:"
echo "- API Swagger: https://localhost:7219/swagger"
echo "- Web Application: http://localhost:5029"
