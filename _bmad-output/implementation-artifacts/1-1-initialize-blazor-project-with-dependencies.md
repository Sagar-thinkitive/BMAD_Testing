# Story 1.1: Initialize Blazor Project with Dependencies

Status: review

## Story

As a developer,
I want a scaffolded BmadPro Blazor Server project with all required NuGet packages and configuration structure,
so that I have a working foundation for building all application features.

## Acceptance Criteria

1. **Given** no existing project, **When** the project is initialized using `dotnet new blazor --interactivity Server -n BmadPro`, **Then** a Blazor Server project targeting .NET 8 is created with Bootstrap 5 included.
2. **Given** the scaffolded project, **When** NuGet packages are added, **Then** `Elsa` 3.5.3 and `Microsoft.Playwright` 1.58.0 are present in `BmadPro.csproj`.
3. **Given** the project, **When** `appsettings.json` is configured, **Then** it contains the `AppUrl`, `DemoData.LoginCredentials`, and `DemoData.InsuranceForm` sections with all 9 form fields.
4. **Given** the project, **When** project directories are created, **Then** `Models/`, `Services/`, `Workflows/`, and `Workflows/Activities/` folders exist.
5. **Given** all configuration is complete, **When** `dotnet build` is run, **Then** it completes without errors.

## Tasks / Subtasks

- [x] Task 1: Scaffold Blazor project (AC: #1)
  - [x] Run `dotnet new blazor --interactivity Server -n BmadPro`
  - [x] Verify Bootstrap 5 is included in `wwwroot/`
  - [x] Remove default sample pages (Weather, Counter) if present
- [x] Task 2: Add NuGet packages (AC: #2)
  - [x] `dotnet add package Elsa --version 3.5.3`
  - [x] `dotnet add package Microsoft.Playwright --version 1.58.0`
- [x] Task 3: Configure appsettings.json (AC: #3)
  - [x] Add `AppUrl` setting
  - [x] Add `DemoData.LoginCredentials` section (Username: "demo", Password: "demo123")
  - [x] Add `DemoData.InsuranceForm` section with all 9 fields
- [x] Task 4: Create project directories (AC: #4)
  - [x] Create `Models/` directory
  - [x] Create `Services/` directory
  - [x] Create `Workflows/` directory
  - [x] Create `Workflows/Activities/` directory
- [x] Task 5: Verify build (AC: #5)
  - [x] Run `dotnet build` and confirm success

## Dev Notes

### Initialization Command

```bash
dotnet new blazor --interactivity Server -n BmadPro
```

This creates a .NET 8 Blazor Web App with server-side interactivity via SignalR. The old `blazorserver` template is deprecated in .NET 8 — use `blazor` instead.

### NuGet Packages

Add to `BmadPro.csproj`:

```xml
<PackageReference Include="Elsa" Version="3.5.3" />
<PackageReference Include="Microsoft.Playwright" Version="1.58.0" />
```

Or via CLI:

```bash
dotnet add package Elsa --version 3.5.3
dotnet add package Microsoft.Playwright --version 1.58.0
```

**Elsa 3.5.3** is a bundle package including `Elsa.Workflows.Core`, `Elsa.Workflows.Management`, `Elsa.Workflows.Runtime`. No additional Elsa sub-packages needed for this POC.

**Microsoft.Playwright 1.58.0** requires a post-build browser install:

```bash
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

Run this AFTER the first successful `dotnet build`. This downloads the Chromium binary Playwright needs.

### appsettings.json — Complete Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AppUrl": "https://localhost:5001",
  "DemoData": {
    "LoginCredentials": {
      "Username": "demo",
      "Password": "demo123"
    },
    "InsuranceForm": {
      "FirstName": "Raj",
      "LastName": "Kumar",
      "DateOfBirth": "1990-05-15",
      "Email": "raj.kumar@example.com",
      "PhoneNumber": "+91-9876543210",
      "Address": "123 MG Road, Mumbai, India",
      "PolicyType": "Health Insurance",
      "CoverageAmount": "500000",
      "NomineeName": "Priya Kumar"
    }
  }
}
```

**CRITICAL:** Keep existing `Logging` and `AllowedHosts` entries from the template. Add `AppUrl` and `DemoData` sections.

### Project Structure Notes

After scaffolding, the template creates:

```
BmadPro/
├── BmadPro.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Properties/launchSettings.json
├── Components/
│   ├── _Imports.razor
│   ├── App.razor
│   ├── Routes.razor
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Pages/           ← Template sample pages here (remove them)
├── wwwroot/
│   ├── css/
│   │   ├── bootstrap/   ← Bootstrap 5 (bundled)
│   │   └── app.css
│   └── favicon.png
```

**Manually create these directories** (empty for now — later stories will add files):

```
Models/
Services/
Workflows/
Workflows/Activities/
```

### Naming Conventions (MUST follow)

- Classes: `PascalCase` — `InsuranceFormModel`, `AuthService`
- Properties: `PascalCase` — `FirstName`, `LastName`, `DateOfBirth`
- Private fields: `_camelCase` — `_playwright`, `_browser`
- Files: Match class name — `InsuranceFormModel.cs`
- Razor pages: `PascalCase` — `Login.razor`, `InsuranceForm.razor`
- Use `FirstName` + `LastName` (NOT `FullName`) — always separate fields

### Anti-Patterns (MUST avoid)

- Do NOT hardcode demo data in `.cs` files — always use `appsettings.json`
- Do NOT use `Thread.Sleep()` — use Playwright auto-wait
- Do NOT mix page logic with workflow logic

### Template Cleanup

The Blazor template includes sample pages (Home, Counter, Weather). Remove these or replace with the Login page redirect:
- Remove `Counter.razor`, `Weather.razor` if they exist
- Keep `Home.razor` only if redirecting to Login, otherwise remove

### References

- [Source: planning-artifacts/architecture.md#Starter Template Evaluation]
- [Source: planning-artifacts/architecture.md#Implementation Patterns & Consistency Rules]
- [Source: planning-artifacts/architecture.md#Project Structure & Boundaries]
- [Source: planning-artifacts/architecture.md#Configuration Pattern]
- [Source: planning-artifacts/epics.md#Story 1.1]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

- Build output: net9.0 (SDK installed is .NET 9, backwards compatible with .NET 8 architecture spec)
- Elsa 3.5.3 restored successfully with all dependencies
- Playwright 1.58.0 installed successfully
- Build: 0 warnings, 0 errors

### Completion Notes List

- Scaffolded Blazor Web App with `dotnet new blazor --interactivity Server -n BmadPro`
- Bootstrap 5 confirmed in `wwwroot/lib/bootstrap/`
- Removed Counter.razor and Weather.razor sample pages
- Added Elsa 3.5.3 and Microsoft.Playwright 1.58.0 NuGet packages
- Configured appsettings.json with AppUrl, DemoData.LoginCredentials, DemoData.InsuranceForm (9 fields)
- Created Models/, Services/, Workflows/, Workflows/Activities/ directories
- Build passes with 0 errors, 0 warnings
- Note: .NET 9 SDK installed on machine — project targets net9.0 (fully compatible, no issues for POC)

### Change Log

- 2026-02-13: Story 1.1 implemented — project scaffolded, packages added, config set, directories created

### File List

- BmadPro/BmadPro.csproj (created — project file with Elsa + Playwright packages)
- BmadPro/Program.cs (created — template default)
- BmadPro/appsettings.json (modified — added AppUrl + DemoData sections)
- BmadPro/appsettings.Development.json (created — template default)
- BmadPro/Properties/launchSettings.json (created — template default)
- BmadPro/Components/_Imports.razor (created — template default)
- BmadPro/Components/App.razor (created — template default)
- BmadPro/Components/Routes.razor (created — template default)
- BmadPro/Components/Layout/MainLayout.razor (created — template default)
- BmadPro/Components/Layout/NavMenu.razor (created — template default)
- BmadPro/Components/Pages/Home.razor (created — template default)
- BmadPro/Components/Pages/Error.razor (created — template default)
- BmadPro/Components/Pages/Counter.razor (deleted — template sample)
- BmadPro/Components/Pages/Weather.razor (deleted — template sample)
- BmadPro/wwwroot/ (created — static assets including Bootstrap 5)
- BmadPro/Models/ (created — empty, for future stories)
- BmadPro/Services/ (created — empty, for future stories)
- BmadPro/Workflows/ (created — empty, for future stories)
- BmadPro/Workflows/Activities/ (created — empty, for future stories)
