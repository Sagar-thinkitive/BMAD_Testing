# Story 3.3: Implement Insurance Form Automation Activity

Status: review

## Story

As a demo presenter,
I want Playwright to automatically fill all 9 insurance form fields and submit,
so that the client watches the form being completed in real-time.

## Acceptance Criteria

1. **Given** the browser is on the Insurance Form page after successful login, **When** `FillInsuranceFormActivity` executes, **Then** Playwright waits for the Blazor InteractiveServer SignalR circuit to connect before interacting with form elements (FR23, NFR4)
2. **Given** the form is ready, **When** `FillInsuranceFormActivity` fills fields, **Then** all 9 fields are filled with demo data from `appsettings.json` `DemoData:InsuranceForm` section: FirstName, LastName, DateOfBirth, Email, PhoneNumber, Address, PolicyType (selects from dropdown), CoverageAmount, NomineeName (FR21)
3. **Given** all fields are filled, **When** the Submit button is clicked, **Then** the form submits successfully and Playwright waits for navigation to the `/form-submitted` confirmation page (FR22)
4. **Given** an error occurs during form automation, **When** the activity catches the exception, **Then** the error is logged via `ILogger<FillInsuranceFormActivity>` and re-thrown for the workflow engine to handle
5. **Given** SlowMo=300 is already configured on the browser, **Then** a 300ms visible delay is applied between each Playwright action for demo observation (NFR2)

## Tasks / Subtasks

- [x] Task 1: Replace FillInsuranceFormActivity placeholder with full Playwright implementation (AC: 1, 2, 3, 4, 5)
  - [x] 1.1: Add Blazor circuit synchronization — `WaitForFunctionAsync("() => window.Blazor !== undefined")` + 1s stabilization delay
  - [x] 1.2: Fill all 9 form fields using Locator-based API with config-driven demo data from `DemoData:InsuranceForm:*`
  - [x] 1.3: Use `SelectOptionAsync()` for PolicyType dropdown (NOT `FillAsync()`)
  - [x] 1.4: Use `FillAsync()` with ISO 8601 format (YYYY-MM-DD) for DateOfBirth field
  - [x] 1.5: Click `#submitButton` and wait for navigation to `**/form-submitted` via `WaitForURLAsync()`
  - [x] 1.6: Wrap all operations in try/catch with `ILogger.LogError()` and re-throw
- [x] Task 2: Verify build passes (AC: all)
  - [x] 2.1: Run `dotnet build` — 0 errors, 0 warnings

## Dev Notes

### Execution Context

This is the **3rd activity** in the `InsuranceAutomationWorkflow` sequence:
```
LaunchBrowserActivity → FillLoginFormActivity → **FillInsuranceFormActivity** → TakeScreenshotActivity → CloseBrowserActivity
```

**Pre-conditions (guaranteed by FillLoginFormActivity):**
- `session.Page` is non-null and active
- Browser is at `/insurance-form` URL
- Auth cookie is set (login succeeded)
- `SlowMo = 300` is already active on the browser (set in LaunchBrowserActivity)

**Post-conditions (required for TakeScreenshotActivity):**
- Form submitted successfully
- Page navigated to `/form-submitted`
- Confirmation page is visible

### Elsa 3.x Activity Pattern (CRITICAL)

**NO constructor injection** — Elsa 3.x does not support it. Resolve ALL services from context:
```csharp
var session = context.GetRequiredService<PlaywrightSession>();
var config = context.GetRequiredService<IConfiguration>();
var logger = context.GetRequiredService<ILogger<FillInsuranceFormActivity>>();
```

Class structure: inherit `CodeActivity`, override `async ValueTask ExecuteAsync(ActivityExecutionContext context)`.

### Playwright .NET 1.58.0 API Rules

**Use Locator-based API ONLY** (deprecated Page-level methods are forbidden):
- `page.Locator(selector).FillAsync(value)` — text fields (auto-waits for visible+enabled+editable)
- `page.Locator(selector).SelectOptionAsync(value)` — dropdown `<select>` elements
- `page.Locator(selector).ClickAsync()` — buttons (auto-waits for visible+stable+enabled)
- `page.WaitForURLAsync(pattern)` — detect navigation completion
- `page.WaitForFunctionAsync(js)` — wait for JS condition (Blazor circuit)

**Anti-patterns to AVOID:**
- ❌ `Page.FillAsync()` / `Page.ClickAsync()` — deprecated Page-level methods
- ❌ `RunAndWaitForNavigationAsync()` — deprecated
- ❌ `Thread.Sleep()` — use `Task.Delay()` only when absolutely needed
- ❌ Manual delays between fills — SlowMo=300 handles demo pacing automatically

### Blazor InteractiveServer Synchronization (CRITICAL)

InsuranceForm.razor uses `@rendermode InteractiveServer`. The page renders as static SSR first, then Blazor enhances it via SignalR. **If you fill fields before the circuit connects, Blazor re-renders and WIPES all values.**

**Required synchronization before ANY form interaction:**
```csharp
await page.WaitForFunctionAsync("() => window.Blazor !== undefined");
await Task.Delay(1000); // Stabilization buffer for circuit activation
```

### Form Field Selectors and Fill Methods

All selectors are stable HTML `id` attributes from `InsuranceForm.razor`:

| Field | Selector | Method | Config Key | Demo Value |
|-------|----------|--------|-----------|------------|
| First Name | `#firstName` | `FillAsync()` | `DemoData:InsuranceForm:FirstName` | "Raj" |
| Last Name | `#lastName` | `FillAsync()` | `DemoData:InsuranceForm:LastName` | "Kumar" |
| Date of Birth | `#dateOfBirth` | `FillAsync()` | `DemoData:InsuranceForm:DateOfBirth` | "1990-05-15" |
| Email | `#email` | `FillAsync()` | `DemoData:InsuranceForm:Email` | "raj.kumar@example.com" |
| Phone Number | `#phoneNumber` | `FillAsync()` | `DemoData:InsuranceForm:PhoneNumber` | "+91-9876543210" |
| Policy Type | `#policyType` | `SelectOptionAsync()` | `DemoData:InsuranceForm:PolicyType` | "Health Insurance" |
| Address | `#address` | `FillAsync()` | `DemoData:InsuranceForm:Address` | "123 MG Road, Mumbai, India" |
| Coverage Amount | `#coverageAmount` | `FillAsync()` | `DemoData:InsuranceForm:CoverageAmount` | "500000" |
| Nominee Name | `#nomineeName` | `FillAsync()` | `DemoData:InsuranceForm:NomineeName` | "Priya Kumar" |
| Submit Button | `#submitButton` | `ClickAsync()` | N/A | N/A |

**Special handling:**
- **PolicyType** (`#policyType`): This is a `<select>` dropdown (Blazor `InputSelect`). MUST use `SelectOptionAsync()`, NOT `FillAsync()`. Value must exactly match an `<option value="...">` attribute.
- **DateOfBirth** (`#dateOfBirth`): This is an `<input type="date">` (Blazor `InputDate`). Use `FillAsync()` with ISO 8601 format `YYYY-MM-DD`. Config already stores `"1990-05-15"`.

### Navigation Pattern After Submit

The insurance form uses **Blazor client-side navigation** (different from login's HTTP 302 redirect):
1. `ClickAsync()` on submit button triggers Blazor `OnValidSubmit` → `HandleValidSubmit()`
2. Handler stores data in `InsuranceFormStateService` (scoped service)
3. Handler calls `Navigation.NavigateTo("/form-submitted")` — client-side redirect
4. Use `page.WaitForURLAsync("**/form-submitted")` to detect navigation complete

### Reference Implementation: FillLoginFormActivity

Follow the exact same patterns established in `FillLoginFormActivity.cs`:
- Same class structure, namespace, imports
- Same DI resolution pattern
- Same Locator API usage
- Same error handling (try/catch + LogError + throw)
- Same logging style (`LogInformation` for milestones)

[Source: BmadPro/Workflows/Activities/FillLoginFormActivity.cs]

### Previous Story Learnings (Story 3.2)

- Locator-based API already correctly implemented in login activity — follow same pattern
- `WaitForURLAsync()` handles both HTTP redirects and Blazor navigations
- `session.Page!` with null-forgiving operator is safe (LaunchBrowserActivity guarantees non-null)
- No additional `Task.Delay()` needed between fills — SlowMo provides demo pacing

### Project Structure Notes

- **File to modify:** `BmadPro/Workflows/Activities/FillInsuranceFormActivity.cs` (replace placeholder)
- **Namespace:** `BmadPro.Workflows.Activities`
- **No new files needed** — only replace placeholder content
- **No changes to Program.cs** — activity already registered via `AddActivitiesFrom<Program>()`
- **No changes to InsuranceAutomationWorkflow.cs** — activity already in sequence

### References

- [Source: _bmad-output/planning-artifacts/architecture.md — Playwright Activity Pattern]
- [Source: _bmad-output/planning-artifacts/epics.md — Epic 3, Story 3.3]
- [Source: _bmad-output/planning-artifacts/prd.md — FR21, FR22, FR23, NFR2, NFR4]
- [Source: BmadPro/Workflows/Activities/FillLoginFormActivity.cs — Reference implementation]
- [Source: BmadPro/Components/Pages/InsuranceForm.razor — Form field IDs]
- [Source: BmadPro/appsettings.json — DemoData:InsuranceForm section]
- [Source: BmadPro/Models/InsuranceFormModel.cs — Field definitions]
- [Source: _bmad-output/implementation-artifacts/3-2-implement-login-automation-activity.md — Previous story learnings]

## Change Log

- 2026-02-19: Initial implementation verified against story tasks. Build passes.
- 2026-02-19: Runtime testing revealed two critical bugs requiring fixes:
  1. **Blazor @bind-Value not receiving Playwright FillAsync values** — Fixed by dispatching bubbling `input` + `change` events via JavaScript after FillAsync, plus Tab key press for backup blur trigger. Blazor's event delegation (blazor.web.js) requires events to bubble to the document root to be processed through SignalR.
  2. **InsuranceFormStateService Scoped→Singleton** — `NavigateTo("/form-submitted")` in .NET 9 Blazor enhanced navigation creates a new DI scope for the destination page. The Scoped InsuranceFormStateService instance was `null` on the FormSubmitted page. Fixed by changing registration to Singleton in Program.cs.
- 2026-02-19: Full end-to-end runtime validation passed — all 9 fields filled, form submitted, confirmation page loaded with correct data, screenshot captured (55650 bytes).

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (claude-opus-4-6)

### Debug Log References

- Build output: `dotnet build` succeeded with 0 warnings, 0 errors (net9.0)
- Runtime log: All 9 fields filled, `modified=9, valid=9, invalid=0` (Blazor model populated)
- Runtime log: `HandleValidSubmit` called with correct values (FirstName='Raj', LastName='Kumar', Email='raj.kumar@example.com')
- Runtime log: FormSubmitted page loaded with FormData present
- Runtime log: Screenshot saved: confirmation_20260219_191439.png (55650 bytes)

### Root Cause Analysis

**Bug 1 — Blazor @bind-Value + Playwright FillAsync:**
Playwright's `FillAsync()` dispatches synthetic `input`/`change` events, but these have `isTrusted: false`. Blazor's `blazor.web.js` event delegation registers listeners at the document root and may not process untrusted synthetic events through the SignalR circuit. The fix: after `FillAsync()`, manually dispatch `new Event('input', { bubbles: true })` and `new Event('change', { bubbles: true })` via `page.EvaluateAsync()`, then `PressAsync("Tab")` for native blur.

**Bug 2 — Scoped service lost on enhanced navigation:**
In .NET 9 Blazor, `NavigateTo()` performs enhanced navigation by default. When navigating between InteractiveServer pages, the destination page may get a new DI scope. `InsuranceFormStateService` was registered as Scoped, so the FormSubmitted page got a fresh instance with `FormData = null`, causing it to redirect back to `/insurance-form`. The fix: register as Singleton since this is a POC/demo app.

### Completion Notes List

- All 9 form fields filled using Locator-based API with config-driven demo data
- PolicyType uses `SelectOptionAsync()` (correct for `<select>` dropdown)
- DateOfBirth uses `FillAsync()` with ISO 8601 format from config
- Blazor circuit synchronization: `WaitForFunctionAsync("() => window.Blazor !== undefined")` + 2s delay
- Blazor binding fix: FillAsync + JS dispatchEvent(input+change) + Tab key for each field
- Submit button click + wait for `h3.text-success` heading (not URL-based — enhanced navigation)
- try/catch with `ILogger.LogError()` and re-throw pattern
- InsuranceFormStateService changed from Scoped to Singleton in Program.cs
- Build passes: 0 errors, 0 warnings on .NET 9.0.203
- Runtime validation: Full end-to-end workflow completed successfully

### File List

- BmadPro/Workflows/Activities/FillInsuranceFormActivity.cs (modified — Blazor binding fix + clean implementation)
- BmadPro/Program.cs (modified — InsuranceFormStateService Scoped→Singleton)
