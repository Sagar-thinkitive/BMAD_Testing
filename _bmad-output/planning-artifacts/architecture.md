---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
lastStep: 8
status: 'complete'
completedAt: '2026-02-13'
inputDocuments:
  - planning-artifacts/prd.md
workflowType: 'architecture'
project_name: 'BmadPro'
user_name: 'LNV-218'
date: '2026-02-13'
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**
26 FRs across 5 capability areas. Authentication (FR1-FR5) covers simple in-memory credential validation. Insurance Form (FR6-FR14) covers 8 form fields with submission. Confirmation (FR15-FR16) displays submitted data. Workflow Orchestration (FR17-FR23) covers the Elsa-driven Playwright automation sequence. Screenshot Capture (FR24-FR26) handles file I/O.

**Non-Functional Requirements:**
9 NFRs across 3 categories. Performance (NFR1-NFR3): 2s page load, 300-500ms field-fill delay, 30s total workflow. Reliability (NFR4-NFR6): auto-wait, repeatable runs, screenshot completion guarantee. Usability (NFR7-NFR9): Bootstrap 5 polish, headed browser, single F5 launch.

**Scale & Complexity:**
- Primary domain: Full-stack web + workflow automation
- Complexity level: Low (POC)
- Estimated architectural components: 4 (Blazor web app, Elsa Workflow engine, Playwright automation layer, screenshot service)

### Technical Constraints & Dependencies

- Single .NET 8 solution housing both the Blazor web app and the Elsa + Playwright automation
- Blazor Server must be fully started and listening before Elsa Workflow launches Playwright
- Playwright targets the locally running Blazor app URL (localhost)
- No database — all state is in-memory
- Chromium must be installed (user confirmed available)
- Screenshot output folder: `InsuranceScreenshot/` in project root

### Cross-Cutting Concerns Identified

- **Startup sequencing:** Blazor app must be ready before workflow triggers browser automation
- **URL coordination:** Playwright needs to know the Blazor app's base URL
- **Demo pacing:** Intentional delays between field fills for visual effect
- **In-memory auth:** Login state must persist across Blazor pages within the session

## Starter Template Evaluation

### Primary Technology Domain

.NET 8 full-stack web + workflow automation. Tech stack defined in PRD: C#, Blazor Server, Bootstrap 5, Elsa Workflow, Playwright.

### Starter Options Considered

The standard .NET 8 Blazor Web App template is the only viable option — it's the official Microsoft template and includes Bootstrap 5 out of the box. The old `blazorserver` template is deprecated in .NET 8.

### Selected Starter: .NET 8 Blazor Web App

**Rationale:** Official Microsoft template, includes Bootstrap 5, supports server-side interactivity, actively maintained, and is the recommended starting point for all new Blazor development in .NET 8+.

**Initialization Command:**

```bash
dotnet new blazor --interactivity Server -n BmadPro
```

**Architectural Decisions Provided by Starter:**

**Language & Runtime:**
C# 12, .NET 8, server-side rendering with SignalR

**Styling Solution:**
Bootstrap 5 (bundled with template by default)

**Build Tooling:**
MSBuild via `dotnet build`, Hot Reload for development

**Code Organization:**
`/Components/Pages/` for Razor pages, `/Components/Layout/` for shared layout

**Development Experience:**
Hot reload, Visual Studio 2022 debugging, `dotnet watch` support

**Additional NuGet Packages Required:**

```xml
<PackageReference Include="Elsa" Version="3.5.3" />
<PackageReference Include="Microsoft.Playwright" Version="1.58.0" />
```

**Verified Package Versions (as of 2026-02-13):**
- Elsa Workflows: 3.5.3 (stable)
- Microsoft.Playwright: 1.58.0 (stable)

**Note:** Project initialization using this command should be the first implementation story.

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**
- Startup sequencing: `IHostApplicationLifetime.ApplicationStarted` event triggers Elsa Workflow
- Auth approach: ASP.NET Core Cookie Authentication (built-in middleware)
- Demo data source: `appsettings.json` configuration

**Important Decisions (Shape Architecture):**
- Component structure: 3 Razor page components in `/Components/Pages/`
- Form handling: Blazor `EditForm` with `DataAnnotationsValidator`
- Elsa integration: Registered as `IHostedService`

**Deferred Decisions (Post-MVP):**
- Database selection, CI/CD pipeline, cloud hosting, logging framework, monitoring

### Data Architecture

- **Storage:** No database. In-memory only.
- **Demo Data:** Stored in `appsettings.json` — demo credentials and form fill data configurable without recompilation
- **Form Model:** C# class with `DataAnnotations` validation attributes
- **Rationale:** Config-driven demo data allows easy customization per client presentation

### Authentication & Security

- **Method:** ASP.NET Core Cookie Authentication
- **Implementation:** Built-in `AuthenticationStateProvider` with cookie middleware
- **Credentials:** Hardcoded in `appsettings.json`, validated server-side
- **Page Protection:** `[Authorize]` attribute on Insurance Form and Confirmation pages
- **Rationale:** Standard Blazor auth pattern, professional code structure, minimal setup

### Startup & Communication

- **Sequencing:** `IHostApplicationLifetime.ApplicationStarted` event triggers the Elsa Workflow after Blazor is fully ready
- **URL Coordination:** Base URL read from `appsettings.json` or `IServer.Features` (Kestrel address)
- **Rationale:** Official .NET lifecycle event — no arbitrary delays, reliable startup detection

### Frontend Architecture

- **Pages:** 3 Razor components — `Login.razor`, `InsuranceForm.razor`, `FormSubmitted.razor`
- **Layout:** Shared `MainLayout.razor` with Bootstrap 5 navbar/container
- **Forms:** Blazor `EditForm` + `DataAnnotationsValidator` + `ValidationSummary`
- **State:** In-memory — form data passed via navigation parameters or scoped service
- **Rationale:** Standard Blazor patterns, no external libraries needed for POC

### Infrastructure & Deployment

- **Hosting:** Localhost only (Visual Studio F5 / `dotnet run`)
- **No CI/CD:** POC — manual build and run
- **No Containers:** Direct execution on developer machine
- **Rationale:** POC scope — deployment infrastructure is post-MVP

### Decision Impact Analysis

**Implementation Sequence:**
1. Project scaffolding (`dotnet new blazor`)
2. Add NuGet packages (Elsa 3.5.3, Playwright 1.58.0)
3. Configure `appsettings.json` with demo data and base URL
4. Implement Cookie Authentication
5. Build 3 Blazor pages (Login → Form → Confirmation)
6. Register Elsa Workflow as hosted service
7. Implement Playwright automation activities
8. Wire startup sequencing via `ApplicationStarted`
9. Add screenshot capture to `InsuranceScreenshot/`

**Cross-Component Dependencies:**
- Elsa Workflow depends on Blazor app being fully started (ApplicationStarted event)
- Playwright depends on Elsa providing the automation sequence
- Screenshot capture depends on Playwright reaching the confirmation page
- Auth cookie must persist across page navigations for Playwright to reach the form

## Implementation Patterns & Consistency Rules

### Naming Patterns

**C# Code Conventions:**
- Classes: `PascalCase` — `InsuranceFormModel`, `AuthService`, `FillLoginFormActivity`
- Properties: `PascalCase` — `FirstName`, `LastName`, `DateOfBirth`, `PolicyType`
- Private fields: `_camelCase` — `_playwright`, `_browser`, `_authService`
- Methods: `PascalCase` — `FillLoginForm()`, `TakeScreenshot()`, `ValidateCredentials()`
- Files: Match class name — `InsuranceFormModel.cs`, `AuthService.cs`
- Razor pages: `PascalCase` — `Login.razor`, `InsuranceForm.razor`, `FormSubmitted.razor`

### Structure Patterns

**Project Organization:**
```
BmadPro/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor
│   └── Pages/
│       ├── Login.razor
│       ├── InsuranceForm.razor
│       └── FormSubmitted.razor
├── Models/
│   ├── LoginModel.cs
│   └── InsuranceFormModel.cs
├── Services/
│   └── AuthService.cs
├── Workflows/
│   ├── InsuranceAutomationWorkflow.cs
│   └── Activities/
│       ├── FillLoginFormActivity.cs
│       ├── FillInsuranceFormActivity.cs
│       └── TakeScreenshotActivity.cs
├── appsettings.json
├── Program.cs
└── InsuranceScreenshot/   (created at runtime)
```

### Configuration Pattern

**`appsettings.json` structure:**
```json
{
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

### Process Patterns

**Error Handling:**
- `try/catch` in Playwright activities with `Console.WriteLine` logging
- No user-facing error pages (POC — restart on failure)
- Playwright auto-wait handles timing; no custom retry logic needed

**Loading States:**
- No loading spinners needed — pages render server-side via Blazor Server
- Playwright waits for elements via built-in auto-wait

### Enforcement Guidelines

**All AI Agents MUST:**
- Follow C# `PascalCase` conventions for all public members
- Place pages in `/Components/Pages/`, models in `/Models/`, services in `/Services/`
- Place Elsa activities in `/Workflows/Activities/` with descriptive `Activity` suffix
- Read demo data from `appsettings.json` `DemoData` section — never hardcode in C# files
- Use `FirstName` and `LastName` as separate fields (not `FullName`)

### Anti-Patterns

- `fullName` or `full_name` — use `FirstName` + `LastName` separately
- Hardcoded demo data in `.cs` files — always use `appsettings.json`
- Playwright `Thread.Sleep()` — use Playwright's built-in `WaitForSelectorAsync()`
- Mixing page logic with workflow logic — keep Blazor pages and Elsa activities separate

## Project Structure & Boundaries

### Complete Project Directory Structure

```
BmadPro/
├── BmadPro.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Properties/
│   └── launchSettings.json
├── Components/
│   ├── _Imports.razor
│   ├── App.razor
│   ├── Routes.razor
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Pages/
│       ├── Login.razor                    ← FR1-FR5
│       ├── InsuranceForm.razor            ← FR6-FR14
│       └── FormSubmitted.razor            ← FR15-FR16
├── Models/
│   ├── LoginModel.cs                      ← FR1-FR3 data model
│   └── InsuranceFormModel.cs              ← FR6-FR14 data model (FirstName, LastName, ...)
├── Services/
│   └── AuthService.cs                     ← FR4-FR5 credential validation
├── Workflows/
│   ├── InsuranceAutomationWorkflow.cs     ← FR17-FR23 orchestration
│   └── Activities/
│       ├── FillLoginFormActivity.cs       ← FR19-FR20
│       ├── FillInsuranceFormActivity.cs   ← FR21-FR22
│       └── TakeScreenshotActivity.cs      ← FR24-FR26
├── wwwroot/
│   ├── css/
│   │   ├── bootstrap/
│   │   └── app.css
│   └── favicon.png
└── InsuranceScreenshot/                   ← Created at runtime (FR26)
```

### Architectural Boundaries

**Page Boundary:**
- Each `.razor` page handles its own UI rendering and form handling
- Pages communicate via Blazor navigation (`NavigationManager`)
- Form data flows: Login → (auth cookie set) → InsuranceForm → (model passed) → FormSubmitted

**Service Boundary:**
- `AuthService` — single responsibility: validate credentials from `appsettings.json`
- No other services needed for POC

**Workflow Boundary:**
- `InsuranceAutomationWorkflow.cs` — Elsa workflow definition (sequence of activities)
- Each activity in `/Activities/` is self-contained: one Playwright action per activity
- Workflow reads config from `appsettings.json` via DI

**Data Flow:**
```
Program.cs starts → Blazor app ready → ApplicationStarted event fires
    → Elsa Workflow triggers → Playwright launches Chromium
    → Login.razor (fill + submit) → InsuranceForm.razor (fill + submit)
    → FormSubmitted.razor → Screenshot → InsuranceScreenshot/
```

### Requirements to Structure Mapping

| FR Category | Files |
|---|---|
| Authentication (FR1-FR5) | `Login.razor`, `LoginModel.cs`, `AuthService.cs` |
| Insurance Form (FR6-FR14) | `InsuranceForm.razor`, `InsuranceFormModel.cs` |
| Confirmation (FR15-FR16) | `FormSubmitted.razor` |
| Workflow (FR17-FR23) | `InsuranceAutomationWorkflow.cs`, `FillLoginFormActivity.cs`, `FillInsuranceFormActivity.cs` |
| Screenshot (FR24-FR26) | `TakeScreenshotActivity.cs`, `InsuranceScreenshot/` |

### Development Workflow

- **Run:** F5 in Visual Studio or `dotnet run`
- **Build:** `dotnet build`
- **Playwright Install:** `pwsh bin/Debug/net8.0/playwright.ps1 install chromium` (one-time setup)

## Architecture Validation Results

### Coherence Validation

**Decision Compatibility:**
All technology choices are compatible — .NET 8 + Blazor Server + Elsa 3.5.3 + Playwright 1.58.0 all target .NET 8 with no conflicts. Bootstrap 5 bundled with Blazor template. Cookie Authentication is standard Blazor Server pattern.

**Pattern Consistency:**
C# PascalCase throughout, consistent with .NET conventions. Project structure follows Blazor Web App template conventions. `appsettings.json` as single source of truth for all configuration.

**Structure Alignment:**
Pages, Models, Services, Workflows directories cleanly separated. Each boundary has single responsibility. Structure supports all architectural decisions.

### Requirements Coverage Validation

**Functional Requirements:** All 26 FRs mapped to specific files and architectural components. No gaps.

| FR Category | Architectural Support | Status |
|---|---|---|
| Authentication (FR1-FR5) | Cookie Auth + AuthService + Login.razor | Covered |
| Insurance Form (FR6-FR14) | EditForm + InsuranceFormModel + InsuranceForm.razor | Covered |
| Confirmation (FR15-FR16) | FormSubmitted.razor | Covered |
| Workflow (FR17-FR23) | Elsa InsuranceAutomationWorkflow + Activities | Covered |
| Screenshot (FR24-FR26) | TakeScreenshotActivity + InsuranceScreenshot/ | Covered |

**Non-Functional Requirements:** All 9 NFRs addressed architecturally.

| NFR | Architectural Support | Status |
|---|---|---|
| NFR1 (2s page load) | Blazor Server local rendering | Covered |
| NFR2 (300-500ms delay) | Configurable delay in Playwright activities | Covered |
| NFR3 (30s total) | Sequential workflow, no bottlenecks | Covered |
| NFR4 (auto-wait) | Playwright built-in auto-wait | Covered |
| NFR5 (repeatable) | Stateless workflow, in-memory reset | Covered |
| NFR6 (screenshot completion) | TakeScreenshotActivity awaits file write | Covered |
| NFR7 (Bootstrap 5 polish) | Template includes Bootstrap 5 | Covered |
| NFR8 (headed mode) | Playwright Chromium headed config | Covered |
| NFR9 (single F5 launch) | ApplicationStarted event auto-triggers | Covered |

### Gap Analysis Results

**Minor documentation gap:** PRD FR6 says "full name" but architecture uses `FirstName` + `LastName` (2 separate fields). PRD should be updated to reflect 9 form fields instead of 8.

**No critical or blocking gaps found.**

### Architecture Completeness Checklist

**Requirements Analysis**
- [x] Project context thoroughly analyzed
- [x] Scale and complexity assessed
- [x] Technical constraints identified
- [x] Cross-cutting concerns mapped

**Architectural Decisions**
- [x] Critical decisions documented with versions
- [x] Technology stack fully specified
- [x] Integration patterns defined
- [x] Performance considerations addressed

**Implementation Patterns**
- [x] Naming conventions established
- [x] Structure patterns defined
- [x] Configuration patterns specified
- [x] Process patterns documented

**Project Structure**
- [x] Complete directory structure defined
- [x] Component boundaries established
- [x] Integration points mapped
- [x] Requirements to structure mapping complete

### Architecture Readiness Assessment

**Overall Status:** READY FOR IMPLEMENTATION

**Confidence Level:** High — low-complexity POC with well-known technologies and clear boundaries.

**Key Strengths:**
- Simple, focused architecture with no unnecessary complexity
- Clear FR-to-file mapping — every requirement has a home
- Clean separation: Blazor pages / Services / Elsa Workflow / Playwright Activities
- Startup sequencing solved via official .NET lifecycle event

**Areas for Future Enhancement:**
- Logging framework (post-MVP)
- Error recovery UI (post-MVP)
- Configurable workflow steps via Elsa designer (Phase 2)

### Implementation Handoff

**AI Agent Guidelines:**
- Follow all architectural decisions exactly as documented
- Use implementation patterns consistently across all components
- Respect project structure and boundaries
- Refer to this document for all architectural questions
- Use `FirstName` and `LastName` as separate fields throughout

**First Implementation Priority:**
```bash
dotnet new blazor --interactivity Server -n BmadPro
```
Then add NuGet packages: Elsa 3.5.3, Microsoft.Playwright 1.58.0
