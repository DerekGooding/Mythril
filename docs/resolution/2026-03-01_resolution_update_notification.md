# Resolution: Update available

**Date:** 2026-03-01
**Resolved By:** Gemini CLI
**Feedback Source:** Tim

## Technical Solution
Enhanced the CI/CD pipeline to generate a `version.json` file on every deployment containing the commit SHA. Implemented a `VersionService` in Blazor that polls this file every 5 minutes. If the version in the file differs from the version loaded at startup, a notification is shown to the user via the `SnackbarService`.

## Changes Made
- Updated `.github/workflows/deploy.yml` to generate `wwwroot/version.json`.
- Created `Mythril.Blazor\Services\VersionService.cs`.
- Updated `Mythril.Blazor\App.razor` to initialize the version check.
- Updated `Mythril.Blazor\Program.cs` to register the service.

## Verification
- Verified the CI script generates valid JSON.
- Verified `VersionService` handles the initial load correctly.
- Verified that a change in the JSON file triggers the `OnUpdateAvailable` event.
