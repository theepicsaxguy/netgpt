#!/bin/bash
set -e

echo "Building NetGPT Backend..."

# Restore packages
dotnet restore

# Build all projects
dotnet build --configuration Release

echo "Build completed successfully!"
echo ""
echo "To run the application:"
echo "  cd src/NetGPT.API"
echo "  dotnet run"
echo ""
echo "To run with Docker:"
echo "  docker-compose up"
