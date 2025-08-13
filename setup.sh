#!/bin/bash
set -e

echo "=== Setting up .NET 9 SDK environment ==="

# Update package lists
sudo apt-get update

# Install .NET 9 SDK (change version if needed)
sudo apt-get install -y dotnet-sdk-9.0

# Verify installation
dotnet --version

# Restore NuGet packages
dotnet restore

echo "=== Environment ready ==="
