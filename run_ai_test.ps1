# Run AI Headless Test
param (
    [string]$ScenarioFile = "test_scenario.json"
)

dotnet run --project Mythril.Headless/Mythril.Headless.csproj -- $ScenarioFile state.json
if ($LASTEXITCODE -eq 0) {
    Write-Host "Headless test completed successfully." -ForegroundColor Green
    Get-Content state.json
} else {
    Write-Host "Headless test failed." -ForegroundColor Red
    exit 1
}
