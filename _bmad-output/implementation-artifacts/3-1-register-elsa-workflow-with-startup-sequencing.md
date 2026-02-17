# Story 3.1: Register Elsa Workflow with Startup Sequencing

Status: review

## Story

As a demo presenter,
I want Elsa Workflow to automatically trigger after the Blazor app is fully started,
so that the automation begins seamlessly with a single F5 press.

## Acceptance Criteria

1. **Given** the BmadPro application starts via F5 or `dotnet run`, **When** the Blazor app is fully initialized and listening, **Then** `IHostApplicationLifetime.ApplicationStarted` event fires and triggers the `InsuranceAutomationWorkflow` (NFR9), **And** Elsa Workflow is registered in `Program.cs`, **And** the workflow reads the base URL from `appsettings.json` `AppUrl` setting.
2. **Given** the workflow is triggered, **When** it begins execution, **Then** Playwright launches Chromium in headed (visible) mode (FR17, NFR8), **And** the browser window is visible to the demo viewer.

## Tasks / Subtasks

- [x] Task 1: Create PlaywrightSession service (AC: #2)
  - [x] Create `Workflows/PlaywrightSession.cs` — singleton holder for IPlaywright, IBrowser, IPage
  - [x] Implement `IAsyncDisposable` for cleanup
  - [x] Register as singleton in Program.cs
- [x] Task 2: Register Elsa services in Program.cs (AC: #1)
  - [x] Add `builder.Services.AddElsa(...)` with `AddActivitiesFrom<Program>()` and `AddWorkflow<InsuranceAutomationWorkflow>()`
  - [x] Add required `using Elsa.Extensions;` imports
- [x] Task 3: Create InsuranceAutomationWorkflow (AC: #1)
  - [x] Create `Workflows/InsuranceAutomationWorkflow.cs` extending `WorkflowBase`
  - [x] Override `Build(IWorkflowBuilder)` with `Sequence` of activities
  - [x] Include placeholder activities for login, form fill, screenshot (implemented in stories 3.2-3.4)
- [x] Task 4: Create placeholder activities (AC: #1, #2)
  - [x] Create `Workflows/Activities/FillLoginFormActivity.cs` — placeholder CodeActivity (logs message)
  - [x] Create `Workflows/Activities/FillInsuranceFormActivity.cs` — placeholder CodeActivity (logs message)
  - [x] Create `Workflows/Activities/TakeScreenshotActivity.cs` — placeholder CodeActivity (logs message)
- [x] Task 5: Wire startup sequencing (AC: #1)
  - [x] Use `IHostApplicationLifetime.ApplicationStarted` to trigger workflow after app is ready
  - [x] Resolve `IWorkflowRunner` from scoped service provider
  - [x] Add 3-second delay before workflow starts to ensure Kestrel is fully accepting requests
  - [x] Use `Task.Run` for async execution from synchronous event handler
- [x] Task 6: Implement browser launch in workflow (AC: #2)
  - [x] First activity in sequence launches Playwright Chromium in headed mode (`Headless = false`)
  - [x] Set `SlowMo = 300` for demo pacing (NFR2: 300-500ms between actions)
  - [x] Store browser/page in PlaywrightSession
  - [x] Last activity in sequence closes browser via PlaywrightSession.DisposeAsync()
- [x] Task 7: Verify build (AC: #1, #2)
  - [x] Run `dotnet build` — 0 errors, 0 warnings

## Dev Notes

### Elsa 3.x API Patterns (CRITICAL — Elsa 3.x is completely different from Elsa 2.x)

**Registration:**
```csharp
using Elsa.Extensions;

builder.Services.AddElsa(elsa =>
{
    elsa.AddActivitiesFrom<Program>();
    elsa.AddWorkflow<InsuranceAutomationWorkflow>();
});
```

- `AddElsa()` with defaults = in-memory, no persistence, no designer — perfect for POC
- `AddActivitiesFrom<Program>()` auto-discovers all `CodeActivity` subclasses in the assembly
- `AddWorkflow<T>()` registers the workflow class
- No `app.UseWorkflows()` needed — workflow is triggered programmatically, not via HTTP

**Workflow class:**
```csharp
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;

public class InsuranceAutomationWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        builder.Root = new Sequence
        {
            Activities =
            {
                new LaunchBrowserActivity(),
                new FillLoginFormActivity(),
                new FillInsuranceFormActivity(),
                new TakeScreenshotActivity(),
                new CloseBrowserActivity()
            }
        };
    }
}
```

**Custom activity base class:**
```csharp
using Elsa.Workflows;
using Elsa.Extensions;

public class MyActivity : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        // Resolve DI services — NO constructor injection in Elsa 3.x activities
        var config = context.GetRequiredService<IConfiguration>();
        var logger = context.GetRequiredService<ILogger<MyActivity>>();
        // ... do work
    }
}
```

**CRITICAL: No constructor injection** in Elsa 3.x activities. Use `context.GetRequiredService<T>()`.

**Triggering workflow:**
```csharp
using Elsa.Workflows;

var workflowRunner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
var result = await workflowRunner.RunAsync<InsuranceAutomationWorkflow>();
```

### Workflows/PlaywrightSession.cs

```csharp
using Microsoft.Playwright;

namespace BmadPro.Workflows;

public class PlaywrightSession : IAsyncDisposable
{
    public IPlaywright? Playwright { get; set; }
    public IBrowser? Browser { get; set; }
    public IPage? Page { get; set; }

    public async ValueTask DisposeAsync()
    {
        if (Page != null) { await Page.CloseAsync(); Page = null; }
        if (Browser != null) { await Browser.CloseAsync(); Browser = null; }
        Playwright?.Dispose();
        Playwright = null;
    }
}
```

**Why singleton?** In Blazor Server with a single workflow run, the PlaywrightSession lives for the app's lifetime. Each activity resolves it from DI and accesses the shared browser/page. This avoids Elsa's serialization issues with non-serializable objects like IPlaywright.

### Workflows/Activities/LaunchBrowserActivity.cs

```csharp
using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Playwright;

namespace BmadPro.Workflows.Activities;

public class LaunchBrowserActivity : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var session = context.GetRequiredService<PlaywrightSession>();
        var logger = context.GetRequiredService<ILogger<LaunchBrowserActivity>>();

        logger.LogInformation("Launching Playwright Chromium (headed mode)...");

        session.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        session.Browser = await session.Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 300  // NFR2: 300-500ms delay for demo visibility
        });

        var browserContext = await session.Browser.NewContextAsync();
        session.Page = await browserContext.NewPageAsync();

        logger.LogInformation("Browser launched successfully.");
    }
}
```

### Workflows/Activities/CloseBrowserActivity.cs

```csharp
using Elsa.Extensions;
using Elsa.Workflows;

namespace BmadPro.Workflows.Activities;

public class CloseBrowserActivity : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var session = context.GetRequiredService<PlaywrightSession>();
        var logger = context.GetRequiredService<ILogger<CloseBrowserActivity>>();

        logger.LogInformation("Closing browser...");
        await session.DisposeAsync();
        logger.LogInformation("Browser closed. Workflow complete.");
    }
}
```

### Program.cs — Startup Sequencing

```csharp
// After app.MapRazorComponents<App>()... but before app.Run():

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        var workflowRunner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Wait for Kestrel to be fully accepting requests
        await Task.Delay(TimeSpan.FromSeconds(3));

        logger.LogInformation("Starting insurance automation workflow...");
        try
        {
            var result = await workflowRunner.RunAsync<InsuranceAutomationWorkflow>();
            logger.LogInformation("Workflow completed with status: {Status}", result.WorkflowState.Status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Workflow execution failed");
        }
    });
});
```

**Key details:**
- `ApplicationStarted.Register()` takes a synchronous delegate — use `Task.Run` for async
- 3-second delay ensures Kestrel is fully accepting HTTP requests before Playwright navigates
- `using var scope = app.Services.CreateScope()` creates a DI scope for resolving IWorkflowRunner
- `IWorkflowRunner` is the Elsa 3.x service for programmatic, in-process workflow execution
- Fire-and-forget pattern (`_ = Task.Run(...)`) is acceptable for POC

### Placeholder Activities (Stories 3.2-3.4)

Create placeholder activities that just log messages. Stories 3.2, 3.3, and 3.4 will replace the logging with actual Playwright automation.

```csharp
// FillLoginFormActivity.cs — placeholder
public class FillLoginFormActivity : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var logger = context.GetRequiredService<ILogger<FillLoginFormActivity>>();
        logger.LogInformation("[Placeholder] FillLoginFormActivity — will be implemented in Story 3.2");
    }
}
```

### Required Imports in Program.cs

```csharp
using BmadPro.Workflows;
using Elsa.Extensions;
using Elsa.Workflows;
```

### Playwright Installation Reminder

Playwright browsers must be installed before the workflow can run:
```bash
pwsh bin/Debug/net9.0/playwright.ps1 install chromium
```

This should be run once after building. The dev should verify this exists.

### Naming Conventions (MUST follow)

- Workflow: `InsuranceAutomationWorkflow` in `/Workflows/`
- Activities: `*Activity` suffix in `/Workflows/Activities/`
- Session holder: `PlaywrightSession` in `/Workflows/`
- All classes: PascalCase
- Private fields: `_camelCase`

### Anti-Patterns (MUST avoid)

- Do NOT use constructor injection in Elsa activities — use `context.GetRequiredService<T>()`
- Do NOT use `Thread.Sleep()` — use Playwright's `SlowMo` option and auto-wait
- Do NOT hardcode URLs — read `AppUrl` from `IConfiguration`
- Do NOT use `app.UseWorkflows()` — workflow is triggered programmatically
- Do NOT use Elsa persistence/EF Core — POC uses in-memory only

### Previous Story Intelligence

- Program.cs has auth services, login/logout endpoints, InsuranceFormStateService
- Login page is Static SSR with HTML form POST to /api/auth/login
- InsuranceForm.razor is InteractiveServer with EditForm
- FormSubmitted.razor shows 9 fields with confirm* ids
- All Playwright selectors documented: #username, #password, #firstName, etc.
- launchSettings.json HTTPS port is 5001 (matches AppUrl)
- Build passes on net9.0

### References

- [Source: planning-artifacts/architecture.md#Startup & Communication]
- [Source: planning-artifacts/architecture.md#Core Architectural Decisions]
- [Source: planning-artifacts/architecture.md#Project Structure & Boundaries]
- [Source: planning-artifacts/epics.md#Story 3.1]
- [Source: Elsa 3 Documentation — Custom Activities]
- [Source: Elsa 3 Documentation — Running Workflows]
- [Source: Microsoft Learn — IHostApplicationLifetime]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

- Build output: net9.0, 0 warnings, 0 errors
- Initial build had CS0234 error for `Elsa.Workflows.Contracts` namespace — removed unused import, `IWorkflowBuilder` resolved from `Elsa.Workflows` namespace
- Placeholder activities initially had CS1998 warnings (async without await) — changed to non-async returning `ValueTask.CompletedTask`

### Completion Notes List

- Created PlaywrightSession singleton service — holds IPlaywright, IBrowser, IPage across Elsa activities
- Registered Elsa services in Program.cs with `AddElsa()`, `AddActivitiesFrom<Program>()`, `AddWorkflow<InsuranceAutomationWorkflow>()`
- Created InsuranceAutomationWorkflow extending WorkflowBase with 5-activity Sequence: Launch → Login → Form → Screenshot → Close
- Created 3 placeholder activities (FillLoginForm, FillInsuranceForm, TakeScreenshot) — log messages, to be implemented in Stories 3.2-3.4
- Created LaunchBrowserActivity — launches Chromium headed with SlowMo=300, stores in PlaywrightSession
- Created CloseBrowserActivity — disposes PlaywrightSession (closes page, browser, Playwright)
- Wired startup sequencing: IHostApplicationLifetime.ApplicationStarted → Task.Run → 3s delay → IWorkflowRunner.RunAsync
- All activities use `context.GetRequiredService<T>()` (no constructor injection per Elsa 3.x pattern)
- Build passes with 0 errors, 0 warnings

### Change Log

- 2026-02-13: Story 3.1 implemented — Elsa workflow registered with startup sequencing and browser launch

### File List

- BmadPro/Workflows/PlaywrightSession.cs (created — singleton IPlaywright/IBrowser/IPage holder)
- BmadPro/Workflows/InsuranceAutomationWorkflow.cs (created — WorkflowBase with 5-activity Sequence)
- BmadPro/Workflows/Activities/LaunchBrowserActivity.cs (created — launches Chromium headed, SlowMo=300)
- BmadPro/Workflows/Activities/FillLoginFormActivity.cs (created — placeholder for Story 3.2)
- BmadPro/Workflows/Activities/FillInsuranceFormActivity.cs (created — placeholder for Story 3.3)
- BmadPro/Workflows/Activities/TakeScreenshotActivity.cs (created — placeholder for Story 3.4)
- BmadPro/Workflows/Activities/CloseBrowserActivity.cs (created — disposes PlaywrightSession)
- BmadPro/Program.cs (modified — added Elsa registration, PlaywrightSession singleton, startup sequencing)
