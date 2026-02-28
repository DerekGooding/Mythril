# Run AI Headless Test
dotnet run --project Mythril.Headless/Mythril.Headless.csproj -- test_scenario.json state.json
if ($LASTEXITCODE -eq 0) {
    Write-Host "Headless test completed. Current State:" -ForegroundColor Green
    Get-Content state.json
} else {
    Write-Host "Headless test failed." -ForegroundColor Red
}
