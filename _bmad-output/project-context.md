---
project_name: 'BmadPro'
user_name: 'LNV-218'
date: '2026-02-17'
sections_completed: ['technology_stack', 'language_rules', 'framework_rules', 'code_quality', 'testing_rules', 'workflow_rules', 'anti_patterns']
status: 'complete'
rule_count: 51
optimized_for_llm: true
existing_patterns_found: 8
---

# Project Context for AI Agents

_This file contains critical rules and patterns that AI agents must follow when implementing code in this project. Focus on unobvious details that agents might otherwise miss._

---

## Technology Stack & Versions

- **Runtime:** .NET 9 (`net9.0` target framework) — note: architecture doc says .NET 8 but csproj targets net9.0
- **Language:** C# 12 with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- **Web Framework:** Blazor Server (Interactive Server render mode via `AddInteractiveServerComponents`)
- **UI Styling:** Bootstrap 5 (bundled with Blazor Web App template, in `wwwroot/lib/bootstrap`)
- **Workflow Engine:** Elsa Workflows **3.5.3** (`Elsa` NuGet package)
- **Browser Automation:** Microsoft.Playwright **1.58.0**
- **Authentication:** ASP.NET Core Cookie Authentication (built-in, no external identity packages)
- **Browser:** Chromium (installed via `pwsh bin/Debug/net9.0/playwright.ps1 install chromium`)
- **Build:** MSBuild / `dotnet build` — no webpack, vite, or JS build tools
- **App URL:** `https://localhost:5001` (configured in `appsettings.json`)

## Critical Implementation Rules

### Language-Specific Rules (C#)

- **Nullable enabled:** All reference types are nullable-aware — always handle nullable returns, use `?` suffix or null-checks
- **Implicit usings enabled:** Do not manually add `using System;`, `using System.Collections.Generic;` etc. — they are auto-included
- **No `var` ambiguity:** Use explicit types when the type is not obvious from the right-hand side
- **Async/await:** All Playwright calls are async — always `await` them; never use `.Result` or `.Wait()` (causes deadlocks in Blazor Server)
- **Error handling in activities:** Wrap Playwright operations in `try/catch` with `Console.WriteLine` / `ILogger` — no user-facing error pages (POC)
- **No `Thread.Sleep()`:** Use `await Task.Delay()` for async waits or Playwright's built-in `WaitForSelectorAsync()` / auto-wait

### Framework-Specific Rules

#### Blazor Server
- **Render mode:** All interactive pages use `@rendermode InteractiveServer` — do not use Static SSR for pages with forms or auth
- **Auth state:** Use `<CascadingAuthenticationState>` (registered in Program.cs) — never inject `IAuthenticationService` directly in components
- **Navigation:** Use `NavigationManager.NavigateTo()` for redirects inside Blazor components — never `Response.Redirect()`
- **Forms:** Use `EditForm` + `DataAnnotationsValidator` + `ValidationSummary` — do not use plain HTML `<form>` for Blazor-handled forms
- **Login/logout:** Auth is handled via minimal API endpoints (`/api/auth/login`, `/api/auth/logout`) with `DisableAntiforgery()` — NOT via Blazor form posts
- **Page protection:** Use `@attribute [Authorize]` on protected pages (InsuranceForm, FormSubmitted) — not middleware-level restrictions
- **State passing:** Form data flows via `InsuranceFormStateService` (scoped DI) — do not pass complex state via query string or route params

#### Elsa Workflows
- **Activity registration:** Activities are auto-discovered via `elsa.AddActivitiesFrom<Program>()` — new activities in `/Workflows/Activities/` are picked up automatically, no manual registration needed
- **Workflow registration:** Workflows must be explicitly registered via `elsa.AddWorkflow<T>()` in Program.cs
- **Workflow trigger:** The workflow is triggered by `IHostApplicationLifetime.ApplicationStarted` with a `Task.Delay(3s)` — this is intentional to let Kestrel fully start
- **Workflow runner:** Use `IWorkflowRunner.RunAsync<T>()` inside a `CreateScope()` — never resolve `IWorkflowRunner` from the root container
- **No Elsa designer/dashboard:** This POC does not use the Elsa Studio UI — workflow is defined purely in code

#### Playwright
- **Session sharing:** `PlaywrightSession` is registered as `Singleton` — browser instance is shared across activities via DI
- **Auto-wait:** Rely on Playwright's built-in auto-wait for element visibility — do not add manual `WaitForSelectorAsync` unless targeting a specific state
- **Headed mode:** Browser must run in headed (visible) mode for demo purposes — never set `Headless = true`
- **Base URL:** Read from `appsettings.json` `AppUrl` key — never hardcode `localhost` URLs in activity code

### Code Quality & Style Rules

#### Naming Conventions
- **Classes & interfaces:** `PascalCase` — `InsuranceFormModel`, `AuthService`, `FillLoginFormActivity`
- **Public properties:** `PascalCase` — `FirstName`, `LastName`, `DateOfBirth`, `PolicyType`
- **Private fields:** `_camelCase` — `_playwright`, `_browser`, `_authService`
- **Methods:** `PascalCase` — `ValidateCredentials()`, `FillLoginForm()`, `TakeScreenshot()`
- **File names:** Must match class name exactly — `InsuranceFormModel.cs`, `AuthService.cs`
- **Razor pages:** `PascalCase` — `Login.razor`, `InsuranceForm.razor`, `FormSubmitted.razor`
- **Activity classes:** Must end with `Activity` suffix — `FillLoginFormActivity`, `TakeScreenshotActivity`

#### Project Structure (strictly enforced)
```
BmadPro/
├── Components/Pages/       ← Razor page components only
├── Components/Layout/      ← Shared layout components
├── Models/                 ← C# model classes with DataAnnotations
├── Services/               ← Application services (scoped DI)
├── Workflows/              ← Elsa workflow definition
│   └── Activities/         ← Individual Elsa activity classes
└── InsuranceScreenshot/    ← Created at runtime, never commit contents
```
- Never place page logic inside workflow classes or vice versa
- Never place models inside Components or Services folders

#### Configuration Rules
- **All demo data** lives in `appsettings.json` under the `DemoData` section — never hardcode in `.cs` files
- **AppUrl** is read from `appsettings.json` — injected via `IConfiguration`
- `appsettings.Development.json` overrides for local dev only — do not put secrets there either (POC)

#### Middleware Order (Program.cs — critical, do not reorder)
1. `UseAuthentication()`
2. `UseAuthorization()`
3. `UseAntiforgery()`

### Testing Rules

- **No test project exists yet** — this is a POC; automated tests are post-MVP
- **Manual testing approach:** Run with F5 / `dotnet run`, visually verify the Chromium browser completes the full automation flow
- **Screenshot as proof:** `InsuranceScreenshot/` folder should contain a screenshot after each successful run — absence of screenshot means workflow failed
- **Playwright install required before first run:**
  ```
  pwsh bin/Debug/net9.0/playwright.ps1 install chromium
  ```
- **Repeatable runs:** App is stateless (in-memory) — each F5 launch resets all state and re-runs the full workflow
- **If adding tests in future:** Place test project at solution root as `BmadPro.Tests/`, use xUnit (standard .NET test framework)

### Development Workflow Rules

#### Running the App
- **Start:** F5 in Visual Studio 2022 or `dotnet run` from `BmadPro/` directory
- **One-time Playwright setup:** `pwsh bin/Debug/net9.0/playwright.ps1 install chromium`
- **Hot reload:** `dotnet watch` supported for Blazor component changes
- **Build:** `dotnet build` from solution root

#### Git
- **NO COMMITS — EVER:** AI agents must NEVER run `git commit`, `git push`, `git merge`, or any destructive git command on any branch — all version control decisions are made by humans only
- **No staging either:** Do not run `git add` — leave all changes unstaged for human review
- **Solution file:** `BmadPro.sln` at repo root — always open solution, not the project directly
- **Do not commit:** `InsuranceScreenshot/` folder contents (runtime output), `bin/`, `obj/`

#### Multi-Developer Impact Analysis (CRITICAL)
- **Before making any code change**, identify all files that reference the component being modified — use grep/search across the entire solution
- **Check cross-cutting concerns:** Changes to `Program.cs`, `AuthService`, `InsuranceFormStateService`, or `PlaywrightSession` affect the entire app — extra caution required
- **Blazor component changes:** Verify that layout changes in `MainLayout.razor` or `NavMenu.razor` do not break page rendering for Login, InsuranceForm, and FormSubmitted
- **Model changes:** Any change to `InsuranceFormModel.cs` or `LoginModel.cs` must be cross-checked against the Razor page that binds it AND the Playwright activity that fills it
- **Workflow/activity changes:** Changes to any Elsa activity affect the full automation sequence — verify the entire workflow chain (Login → Form → Screenshot) still functions
- **appsettings.json changes:** Any key rename or structural change must be updated in every `IConfiguration` binding across all services and activities
- **Always report impact:** When implementing a story, explicitly state which other files/components were reviewed and confirmed unaffected

#### No CI/CD
- **Manual only:** Build and run locally — no pipelines, containers, or cloud deployment for POC
- **No Docker:** Direct execution on developer machine with .NET 9 SDK installed

#### DI Lifetime Rules (Program.cs)
- `AuthService` → `Scoped` (per-request, reads config)
- `InsuranceFormStateService` → `Scoped` (per-user session, holds form state)
- `PlaywrightSession` → `Singleton` (one browser instance for the app lifetime)
- Never register Playwright-related services as `Transient` — browser instances are expensive

### Critical Don't-Miss Rules

#### Anti-Patterns — Never Do These
- **`fullName` or `full_name`:** Always use `FirstName` + `LastName` as two separate fields throughout models, forms, and Playwright selectors
- **Hardcoded demo data in `.cs` files:** All credentials and form fill values must come from `appsettings.json` `DemoData` section
- **`Thread.Sleep()`:** Blocks the thread in async Blazor Server context — use `await Task.Delay()` or Playwright auto-wait
- **`.Result` or `.Wait()` on Tasks:** Causes deadlocks in Blazor Server's synchronization context — always `await`
- **Resolving `IWorkflowRunner` from root container:** Always use `CreateScope()` — workflow runner requires a scoped lifetime
- **Setting `Headless = true` in Playwright:** Demo requires visible browser — headed mode is a hard requirement
- **Mixing page logic with activity logic:** Blazor `.razor` files handle UI only; Elsa activities handle automation only
- **Reordering middleware:** `UseAuthentication()` must come before `UseAuthorization()` and `UseAntiforgery()` — wrong order breaks auth silently

#### Edge Cases Agents Must Handle
- **Startup race condition:** The `Task.Delay(3s)` in `ApplicationStarted` is intentional — do not remove it; Kestrel needs time to start accepting HTTPS requests
- **Screenshot folder:** `InsuranceScreenshot/` must be created if it doesn't exist before writing — use `Directory.CreateDirectory()` before `SaveAsAsync()`
- **Auth cookie for Playwright:** Playwright must navigate to `/api/auth/login` (POST) or handle the redirect — the login form submits to a minimal API endpoint, not a Blazor handler
- **`net9.0` vs `.NET 8` mismatch:** The csproj targets `net9.0` but architecture docs reference .NET 8 — use `net9.0` as the source of truth for all build commands and Playwright install paths

---

## Usage Guidelines

**For AI Agents:**
- Read this file before implementing any code in this project
- Follow ALL rules exactly as documented — especially middleware order, DI lifetimes, and naming conventions
- When in doubt, prefer the more restrictive option
- Update this file if new patterns emerge during implementation

**For Humans:**
- Keep this file lean and focused on agent needs
- Update when technology stack or patterns change
- Remove rules that become obvious over time

Last Updated: 2026-02-17
