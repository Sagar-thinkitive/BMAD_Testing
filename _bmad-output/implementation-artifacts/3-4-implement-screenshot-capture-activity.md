# Story 3.4: Implement Screenshot Capture Activity

Status: review

## Story

As a demo presenter,
I want a screenshot of the confirmation page saved automatically,
so that there is proof of successful end-to-end automation.

## Acceptance Criteria

1. **Given** the browser is on the FormSubmitted confirmation page after successful form submission, **When** `TakeScreenshotActivity` executes, **Then** Playwright waits for the Blazor InteractiveServer circuit to connect and the confirmation data to render before capturing (FR24, NFR4)
2. **Given** the confirmation page is fully rendered with all 9 field values visible, **When** the screenshot is taken, **Then** a full-page screenshot is captured as a PNG file (FR24)
3. **Given** the `InsuranceScreenshot/` directory does not exist, **When** the activity runs, **Then** the directory is created before saving the screenshot (FR26)
4. **Given** the screenshot is saved, **When** the file write completes, **Then** the PNG file exists on disk in `InsuranceScreenshot/` with a timestamped filename and non-zero file size (FR25, NFR6)
5. **Given** an error occurs during screenshot capture, **When** the activity catches the exception, **Then** the error is logged via `ILogger<TakeScreenshotActivity>` and re-thrown for the workflow engine to handle

## Tasks / Subtasks

- [x] Task 1: Implement TakeScreenshotActivity with full Playwright screenshot logic (AC: 1, 2, 3, 4, 5)
  - [x] 1.1: Add Blazor circuit synchronization — `WaitForFunctionAsync("() => window.Blazor !== undefined")` + stabilization delay
  - [x] 1.2: Wait for confirmation page content — `h3.text-success` heading visible
  - [x] 1.3: Wait for data table — `table.table-striped` visible (confirms all 9 fields rendered)
  - [x] 1.4: Create `InsuranceScreenshot/` directory via `Directory.CreateDirectory()`
  - [x] 1.5: Capture full-page screenshot with `PageScreenshotOptions { Path, FullPage = true }`
  - [x] 1.6: Verify file written to disk — `File.Exists()` + log file size (NFR6)
  - [x] 1.7: Wrap all operations in try/catch with `ILogger.LogError()` and re-throw
- [x] Task 2: Verify build passes (AC: all)
  - [x] 2.1: Run `dotnet build` — 0 errors, 0 warnings

## Dev Notes

### Execution Context

This is the **4th activity** in the `InsuranceAutomationWorkflow` sequence:
```
LaunchBrowserActivity → FillLoginFormActivity → FillInsuranceFormActivity → **TakeScreenshotActivity** → CloseBrowserActivity
```

**Pre-conditions (guaranteed by FillInsuranceFormActivity):**
- `session.Page` is non-null and active
- Browser has navigated to `/form-submitted` confirmation page
- Auth cookie is set (login succeeded earlier)
- `InsuranceFormStateService.FormData` is populated (form was submitted)
- Confirmation page should be showing the success heading and data table

**Post-conditions (required for CloseBrowserActivity):**
- Screenshot PNG file saved to `InsuranceScreenshot/` directory
- File existence verified on disk
- No changes to `session.Page` state needed — CloseBrowserActivity just disposes

### Elsa 3.x Activity Pattern (CRITICAL)

**NO constructor injection** — Elsa 3.x does not support it. Resolve ALL services from context:
```csharp
var session = context.GetRequiredService<PlaywrightSession>();
var logger = context.GetRequiredService<ILogger<TakeScreenshotActivity>>();
```

Class structure: inherit `CodeActivity`, override `async ValueTask ExecuteAsync(ActivityExecutionContext context)`.

### Playwright .NET 1.58.0 Screenshot API

**Screenshot method:**
```csharp
await page.ScreenshotAsync(new PageScreenshotOptions
{
    Path = screenshotPath,
    FullPage = true
});
```

**Key parameters:**
- `Path`: Full filesystem path where PNG will be saved
- `FullPage = true`: Captures entire scrollable page, not just viewport (FR24)
- Default format is PNG — no need to specify format

**Confirmation page selectors to wait for:**
- `h3.text-success` — "Application Submitted Successfully!" heading (proves page loaded)
- `table.table-striped` — Data table with all 9 field values (proves data rendered)

### Blazor InteractiveServer Synchronization (CRITICAL)

FormSubmitted.razor uses `@rendermode InteractiveServer`. The page renders as static SSR first, then Blazor enhances it via SignalR. The confirmation data is rendered server-side through Blazor's circuit.

**Required synchronization before screenshot:**
```csharp
await page.WaitForFunctionAsync("() => window.Blazor !== undefined");
await Task.Delay(1500); // Buffer for Blazor circuit to stabilize and render data
```

**Then wait for specific content:**
```csharp
await page.Locator("h3.text-success").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible
});
await page.Locator("table.table-striped").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible
});
```

### Confirmation Page Structure (FormSubmitted.razor)

The page has a null guard: if `FormState.FormData is null`, it redirects to `/insurance-form`. If data IS present:
- `<h3 class="text-success">Application Submitted Successfully!</h3>` — success heading
- `<table class="table table-striped">` — data table with 9 rows, each containing field label and value
- Field value cells have IDs: `#confirmFirstName`, `#confirmLastName`, etc.

### Screenshot File Naming and Path

```csharp
var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "InsuranceScreenshot");
Directory.CreateDirectory(screenshotDir); // Creates if not exists, no-op if exists (FR26)
var screenshotPath = Path.Combine(screenshotDir, $"confirmation_{DateTime.Now:yyyyMMdd_HHmmss}.png");
```

- Directory: `InsuranceScreenshot/` relative to the app's working directory
- Filename pattern: `confirmation_YYYYMMDD_HHMMSS.png` — timestamped for uniqueness across runs (NFR5)
- `Directory.CreateDirectory()` is idempotent — safe to call every time (FR26)

### File Verification Pattern (NFR6)

After screenshot, verify the file was actually written:
```csharp
if (!File.Exists(screenshotPath))
{
    throw new FileNotFoundException("Screenshot file was not written to disk", screenshotPath);
}
var fileInfo = new FileInfo(screenshotPath);
logger.LogInformation("Screenshot saved to: {Path} ({Size} bytes)", screenshotPath, fileInfo.Length);
```

This satisfies NFR6: "Screenshot file written to disk before workflow reports completion."

### Reference Implementation: FillLoginFormActivity

Follow the exact same patterns established in `FillLoginFormActivity.cs`:
- Same class structure, namespace, imports
- Same DI resolution pattern (`context.GetRequiredService<T>()`)
- Same error handling (try/catch + LogError + throw)
- Same logging style (`LogInformation` for milestones)

[Source: BmadPro/Workflows/Activities/FillLoginFormActivity.cs]

### Previous Story Learnings (Story 3.2 + 3.3)

- Locator-based API correctly used across all activities — follow same pattern
- `session.Page!` with null-forgiving operator is safe (LaunchBrowserActivity guarantees non-null)
- Blazor circuit synchronization is essential for InteractiveServer pages — `WaitForFunctionAsync("() => window.Blazor !== undefined")` + delay
- **CRITICAL from Story 3.3 debugging**: Blazor's `@bind-Value` on InputText requires proper event propagation through SignalR. If form submission fails silently (no navigation), the issue is Blazor model binding not receiving values from Playwright. Story 3.3 is still debugging this — TakeScreenshotActivity depends on 3.3 succeeding.
- `WaitForURLAsync("**/form-submitted")` may fail with "Target page, context or browser has been closed" due to Blazor enhanced navigation in .NET 9 — waiting for specific page content (`h3.text-success`) is more reliable than URL-based waits
- Build uses .NET 9 SDK at `/usr/share/dotnet/` — must set `DOTNET_ROOT=/usr/share/dotnet` in WSL
- Chromium system libraries are installed in WSL — `libnspr4`, `libgbm`, `libcairo`, `libpango`, `libasound2` etc.
- `IgnoreHTTPSErrors = true` is set on browser context in `LaunchBrowserActivity` for self-signed dev cert

### Project Structure Notes

- **File to modify:** `BmadPro/Workflows/Activities/TakeScreenshotActivity.cs` (code already exists, verify/update)
- **Namespace:** `BmadPro.Workflows.Activities`
- **No new files needed** — only modify existing activity
- **No changes to Program.cs** — activity already registered via `AddActivitiesFrom<Program>()`
- **No changes to InsuranceAutomationWorkflow.cs** — activity already in sequence at position 4
- **Runtime directory created:** `InsuranceScreenshot/` — already in `.gitignore`

### References

- [Source: _bmad-output/planning-artifacts/architecture.md — Screenshot (FR24-FR26) section]
- [Source: _bmad-output/planning-artifacts/epics.md — Epic 3, Story 3.4]
- [Source: _bmad-output/planning-artifacts/prd.md — FR24, FR25, FR26, NFR3, NFR5, NFR6]
- [Source: BmadPro/Workflows/Activities/FillLoginFormActivity.cs — Reference implementation pattern]
- [Source: BmadPro/Components/Pages/FormSubmitted.razor — Confirmation page selectors]
- [Source: BmadPro/Workflows/InsuranceAutomationWorkflow.cs — Activity sequence]
- [Source: BmadPro/Workflows/Activities/CloseBrowserActivity.cs — Next activity in chain]
- [Source: _bmad-output/implementation-artifacts/3-3-implement-insurance-form-automation-activity.md — Previous story learnings]
- [Source: _bmad-output/implementation-artifacts/3-2-implement-login-automation-activity.md — Pattern reference]

## Change Log

- 2026-02-19: Story implementation verified and completed. TakeScreenshotActivity already contained full Playwright implementation matching all acceptance criteria. Build verified: 0 errors, 0 warnings on .NET 9.0.203.

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (claude-opus-4-6)

### Debug Log References

- Build output: `dotnet build` succeeded with 0 warnings, 0 errors (net9.0)
- TakeScreenshotActivity.cs verified line-by-line against all 7 subtasks

### Completion Notes List

- TakeScreenshotActivity.cs already contained the complete implementation (not a placeholder)
- Blazor circuit synchronization present: `WaitForFunctionAsync("() => window.Blazor !== undefined")` + 1500ms delay
- Success heading wait: `h3.text-success` with `WaitForSelectorState.Visible`
- Data table wait: `table.table-striped` with `WaitForSelectorState.Visible`
- Directory creation: `Directory.CreateDirectory()` — idempotent (FR26)
- Full-page screenshot: `PageScreenshotOptions { Path, FullPage = true }` (FR24, FR25)
- File verification: `File.Exists()` check + `FileInfo.Length` logging (NFR6)
- Error handling: try/catch with `ILogger.LogError()` and re-throw
- No code changes needed — implementation was already complete and correct
- Build passes: 0 errors, 0 warnings on .NET 9.0.203
- **NOTE:** Runtime validation depends on Story 3.3 (FillInsuranceFormActivity) succeeding — the form submission to `/form-submitted` must work for TakeScreenshotActivity to execute

### File List

- BmadPro/Workflows/Activities/TakeScreenshotActivity.cs (verified — no changes needed, implementation already complete)
