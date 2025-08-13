# Jules Agent Configuration

## Project Overview
This project is built with **.NET 9** and requires the .NET SDK in the development environment before any commands can be run.  
All build, test, and run operations should be performed inside a shell where the .NET SDK is installed.

## Environment Setup
When initializing the environment, run the following commands:

```bash
#!/bin/bash
set -e

# Install .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0

# Restore NuGet packages
dotnet restore
