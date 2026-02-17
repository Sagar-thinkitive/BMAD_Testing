# Story 2.1: Implement Insurance Form Page with Validation

Status: review

## Story

As a demo presenter,
I want a professional Bootstrap 5 insurance form with 9 validated fields,
so that the demo showcases a realistic data-entry experience.

## Acceptance Criteria

1. **Given** an authenticated user on the Insurance Form page, **When** the page renders, **Then** `InsuranceForm.razor` displays a Bootstrap 5 styled form with these fields: FirstName (text), LastName (text), DateOfBirth (date), Email (email), PhoneNumber (text), Address (text), PolicyType (dropdown with predefined options), CoverageAmount (text), NomineeName (text) (FR6-FR13), **And** the page is protected with `[Authorize]` attribute.
2. **Given** the form is displayed, **When** the user leaves required fields empty and clicks Submit, **Then** `DataAnnotationsValidator` displays validation errors via `ValidationSummary`, **And** the form is not submitted.
3. **Given** all fields are filled with valid data, **When** the user clicks Submit (FR14), **Then** the form data is captured in `InsuranceFormModel` and the user is navigated to the confirmation page.

## Tasks / Subtasks

- [x] Task 1: Create InsuranceFormModel (AC: #1, #2)
  - [x] Create `Models/InsuranceFormModel.cs` with all 9 fields
  - [x] Add `[Required]` and appropriate DataAnnotations on each field
  - [x] Use `DateTime?` for DateOfBirth (nullable so `[Required]` works)
  - [x] Use `string` for CoverageAmount (matches appsettings.json format)
  - [x] Use `string` for PolicyType with empty default (avoids enum validation bug)
- [x] Task 2: Create InsuranceFormStateService (AC: #3)
  - [x] Create `Services/InsuranceFormStateService.cs` — scoped service to hold form data across pages
  - [x] Register as `AddScoped<InsuranceFormStateService>()` in Program.cs
- [x] Task 3: Create InsuranceForm.razor page (AC: #1, #2, #3)
  - [x] Create `Components/Pages/InsuranceForm.razor` with `@rendermode InteractiveServer`
  - [x] Route: `@page "/insurance-form"`
  - [x] Protected: `@attribute [Authorize]`
  - [x] Use `EditForm` with `Model="@formModel"` and `OnValidSubmit="HandleValidSubmit"`
  - [x] Add `DataAnnotationsValidator` and `ValidationSummary`
  - [x] Bootstrap 5 card layout with all 9 fields using Blazor input components
  - [x] Stable `id` attributes on all inputs for Playwright (firstName, lastName, etc.)
  - [x] PolicyType as `InputSelect` with dropdown options
  - [x] On valid submit: store data in `InsuranceFormStateService`, navigate to `/form-submitted`
- [x] Task 4: Create placeholder FormSubmitted.razor (AC: #3)
  - [x] Create minimal `Components/Pages/FormSubmitted.razor` with `@rendermode InteractiveServer`
  - [x] Route: `@page "/form-submitted"`, protected with `@attribute [Authorize]`
  - [x] Display basic "Form Submitted Successfully" message (full implementation in Story 2.2)
  - [x] Redirect to `/insurance-form` if no form data in state service
- [x] Task 5: Verify build (AC: #1-#3)
  - [x] Run `dotnet build` — 0 errors, 0 warnings

## Dev Notes

### InsuranceForm.razor — Interactive Server Component

Unlike Login.razor (which is Static SSR), the Insurance Form page uses `@rendermode InteractiveServer` because:
- It needs real-time form validation via `EditForm` + `DataAnnotationsValidator`
- Form submission happens over SignalR (no HTTP POST needed — data stays server-side)
- `[Authorize]` works with `AuthorizeRouteView` already configured in Routes.razor

**Key pattern for InteractiveServer forms:**
- Use `<EditForm Model="@formModel" OnValidSubmit="HandleValidSubmit">` — classic Blazor pattern
- Do NOT use `FormName` (that's for SSR forms only)
- Do NOT use `[SupplyParameterFromForm]` (that's SSR model-binding)
- `OnValidSubmit` fires as a C# method via SignalR — no HTTP POST occurs
- Antiforgery is automatic — `EditForm` renders a hidden token, `UseAntiforgery()` middleware handles it

### Models/InsuranceFormModel.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace BmadPro.Models;

public class InsuranceFormModel
{
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Policy type is required")]
    [Display(Name = "Policy Type")]
    public string PolicyType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Coverage amount is required")]
    [Display(Name = "Coverage Amount")]
    public string CoverageAmount { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nominee name is required")]
    [Display(Name = "Nominee Name")]
    public string NomineeName { get; set; } = string.Empty;
}
```

**Key design decisions:**
- `DateOfBirth` is `DateTime?` (nullable) — non-nullable `DateTime` defaults to `DateTime.MinValue` and never fails `[Required]`
- `CoverageAmount` is `string` — matches `appsettings.json` format ("500000") and simplifies Playwright automation (just type the string)
- `PolicyType` is `string` with `string.Empty` default — avoids the [known InputSelect + enum validation bug](https://github.com/dotnet/aspnetcore/issues/27746). Include a `<option value="">-- Select --</option>` placeholder so empty string fails `[Required]`
- All string fields initialize to `string.Empty` to avoid null reference warnings
- Use `FirstName` + `LastName` as separate fields (NOT `FullName`)

### Services/InsuranceFormStateService.cs

```csharp
using BmadPro.Models;

namespace BmadPro.Services;

public class InsuranceFormStateService
{
    public InsuranceFormModel? FormData { get; private set; }

    public void SetFormData(InsuranceFormModel data)
    {
        FormData = data;
    }
}
```

**Why scoped service?** In Blazor Server, a scoped service's lifetime matches the SignalR circuit (user session). Each user gets their own instance. This is cleaner than query parameters (which would be unwieldy with 9 fields) and avoids serialization.

Register in Program.cs alongside existing services:
```csharp
builder.Services.AddScoped<InsuranceFormStateService>();
```

### Components/Pages/InsuranceForm.razor

```razor
@page "/insurance-form"
@rendermode InteractiveServer
@attribute [Authorize]
@using BmadPro.Models
@using BmadPro.Services
@inject InsuranceFormStateService FormState
@inject NavigationManager Navigation

<PageTitle>Insurance Form - BmadPro</PageTitle>

<div class="container mt-4">
    <div class="row justify-content-center">
        <div class="col-md-8">
            <div class="card shadow">
                <div class="card-body p-4">
                    <h3 class="card-title text-center mb-4">Insurance Application Form</h3>

                    <EditForm Model="@formModel" OnValidSubmit="HandleValidSubmit">
                        <DataAnnotationsValidator />
                        <ValidationSummary class="text-danger mb-3" />

                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="firstName" class="form-label">First Name</label>
                                <InputText id="firstName" class="form-control" @bind-Value="formModel.FirstName" />
                                <ValidationMessage For="@(() => formModel.FirstName)" class="text-danger" />
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="lastName" class="form-label">Last Name</label>
                                <InputText id="lastName" class="form-control" @bind-Value="formModel.LastName" />
                                <ValidationMessage For="@(() => formModel.LastName)" class="text-danger" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="dateOfBirth" class="form-label">Date of Birth</label>
                                <InputDate id="dateOfBirth" class="form-control" @bind-Value="formModel.DateOfBirth" />
                                <ValidationMessage For="@(() => formModel.DateOfBirth)" class="text-danger" />
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="email" class="form-label">Email</label>
                                <InputText id="email" class="form-control" type="email" @bind-Value="formModel.Email" />
                                <ValidationMessage For="@(() => formModel.Email)" class="text-danger" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="phoneNumber" class="form-label">Phone Number</label>
                                <InputText id="phoneNumber" class="form-control" type="tel" @bind-Value="formModel.PhoneNumber" />
                                <ValidationMessage For="@(() => formModel.PhoneNumber)" class="text-danger" />
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="policyType" class="form-label">Policy Type</label>
                                <InputSelect id="policyType" class="form-select" @bind-Value="formModel.PolicyType">
                                    <option value="">-- Select Policy Type --</option>
                                    <option value="Health Insurance">Health Insurance</option>
                                    <option value="Life Insurance">Life Insurance</option>
                                    <option value="Auto Insurance">Auto Insurance</option>
                                    <option value="Home Insurance">Home Insurance</option>
                                </InputSelect>
                                <ValidationMessage For="@(() => formModel.PolicyType)" class="text-danger" />
                            </div>
                        </div>

                        <div class="mb-3">
                            <label for="address" class="form-label">Address</label>
                            <InputText id="address" class="form-control" @bind-Value="formModel.Address" />
                            <ValidationMessage For="@(() => formModel.Address)" class="text-danger" />
                        </div>

                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="coverageAmount" class="form-label">Coverage Amount</label>
                                <InputText id="coverageAmount" class="form-control" @bind-Value="formModel.CoverageAmount" />
                                <ValidationMessage For="@(() => formModel.CoverageAmount)" class="text-danger" />
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="nomineeName" class="form-label">Nominee Name</label>
                                <InputText id="nomineeName" class="form-control" @bind-Value="formModel.NomineeName" />
                                <ValidationMessage For="@(() => formModel.NomineeName)" class="text-danger" />
                            </div>
                        </div>

                        <button type="submit" id="submitButton" class="btn btn-primary w-100 mt-2">
                            Submit Application
                        </button>
                    </EditForm>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private InsuranceFormModel formModel = new();

    private void HandleValidSubmit()
    {
        FormState.SetFormData(formModel);
        Navigation.NavigateTo("/form-submitted");
    }
}
```

### Playwright Selector Reference (for Epic 3, Story 3.3)

All form elements have stable `id` attributes:
- `#firstName` — FirstName text input
- `#lastName` — LastName text input
- `#dateOfBirth` — DateOfBirth date input
- `#email` — Email text input
- `#phoneNumber` — PhoneNumber text input
- `#address` — Address text input
- `#policyType` — PolicyType dropdown (select)
- `#coverageAmount` — CoverageAmount text input
- `#nomineeName` — NomineeName text input
- `#submitButton` — Submit button

**Playwright fill pattern for InputSelect (dropdown):**
```csharp
await page.Locator("#policyType").SelectOptionAsync("Health Insurance");
```

**Playwright fill pattern for InputDate:**
```csharp
await page.Locator("#dateOfBirth").FillAsync("1990-05-15");
```

### Placeholder FormSubmitted.razor (Story 2.1 scope)

Create a minimal page that satisfies AC #3 (navigation after submit). Story 2.2 will implement the full confirmation display.

```razor
@page "/form-submitted"
@rendermode InteractiveServer
@attribute [Authorize]
@using BmadPro.Services
@inject InsuranceFormStateService FormState
@inject NavigationManager Navigation

<PageTitle>Form Submitted - BmadPro</PageTitle>

<div class="container mt-5">
    <div class="row justify-content-center">
        <div class="col-md-8">
            <div class="card shadow">
                <div class="card-body p-4 text-center">
                    <h3 class="text-success">Application Submitted Successfully!</h3>
                    <p class="text-muted">Your insurance application has been received.</p>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    protected override void OnInitialized()
    {
        if (FormState.FormData is null)
        {
            Navigation.NavigateTo("/insurance-form");
        }
    }
}
```

### Program.cs — Single Addition

Add after the existing `builder.Services.AddScoped<AuthService>();` line:
```csharp
builder.Services.AddScoped<InsuranceFormStateService>();
```

No other changes to Program.cs needed.

### No Additional NuGet Packages Required

`System.ComponentModel.DataAnnotations` is part of the .NET SDK. All Blazor form components (`EditForm`, `InputText`, `InputDate`, `InputSelect`, `DataAnnotationsValidator`, `ValidationSummary`, `ValidationMessage`) are in `Microsoft.AspNetCore.Components.Forms` — already imported in `_Imports.razor`.

### Naming Conventions (MUST follow)

- Model: `InsuranceFormModel` (PascalCase class)
- Properties: `FirstName`, `LastName`, `DateOfBirth`, `Email`, `PhoneNumber`, `Address`, `PolicyType`, `CoverageAmount`, `NomineeName` (PascalCase)
- Service: `InsuranceFormStateService` (PascalCase class)
- File: `InsuranceFormModel.cs`, `InsuranceFormStateService.cs` (match class name)
- Razor page: `InsuranceForm.razor` (PascalCase)
- HTML ids: `firstName`, `lastName`, `dateOfBirth`, `email`, `phoneNumber`, `address`, `policyType`, `coverageAmount`, `nomineeName` (camelCase for HTML convention)
- Use `FirstName` + `LastName` (NOT `FullName`) — always separate fields

### Anti-Patterns (MUST avoid)

- Do NOT use `FormName` on EditForm — that's for SSR forms only, InteractiveServer doesn't need it
- Do NOT use `[SupplyParameterFromForm]` — that's SSR model-binding, not applicable here
- Do NOT use enum for PolicyType — causes [known validation bug with InputSelect](https://github.com/dotnet/aspnetcore/issues/27746)
- Do NOT use non-nullable `DateTime` for DateOfBirth — `[Required]` won't work (defaults to MinValue)
- Do NOT hardcode demo data in C# files — form model starts empty, Playwright fills from appsettings.json
- Do NOT mix `OnSubmit` with `OnValidSubmit` — they are mutually exclusive on EditForm
- Do NOT pass form data via query parameters — 9 fields makes URLs unwieldy, use scoped service

### Previous Story Intelligence (from Story 1.2)

- Login.razor is Static SSR at routes `/` and `/login`
- Cookie authentication configured with `AuthorizeRouteView` in Routes.razor
- `RedirectToLogin.razor` with `forceLoad: true` handles unauthorized redirects
- _Imports.razor already has auth usings
- launchSettings.json HTTPS port is 5001 (matches appsettings.json AppUrl)
- Build passes with 0 errors, 0 warnings on net9.0

### Project Structure After This Story

```
BmadPro/
├── Components/
│   └── Pages/
│       ├── Login.razor              ← unchanged (Story 1.2)
│       ├── InsuranceForm.razor      ← NEW (InteractiveServer, EditForm)
│       └── FormSubmitted.razor      ← NEW (placeholder, full impl in Story 2.2)
├── Models/
│   ├── LoginModel.cs               ← unchanged (Story 1.2)
│   └── InsuranceFormModel.cs        ← NEW (9 fields with DataAnnotations)
├── Services/
│   ├── AuthService.cs              ← unchanged (Story 1.2)
│   └── InsuranceFormStateService.cs ← NEW (scoped state container)
├── Program.cs                       ← MODIFIED (register InsuranceFormStateService)
└── ...
```

### References

- [Source: planning-artifacts/architecture.md#Frontend Architecture]
- [Source: planning-artifacts/architecture.md#Data Architecture]
- [Source: planning-artifacts/architecture.md#Implementation Patterns & Consistency Rules]
- [Source: planning-artifacts/architecture.md#Anti-Patterns]
- [Source: planning-artifacts/epics.md#Story 2.1]
- [Source: implementation-artifacts/1-2-implement-login-page-with-cookie-authentication.md#Dev Agent Record]
- [Source: Microsoft Learn — Blazor Forms Overview]
- [Source: Microsoft Learn — Blazor Forms Validation]
- [Source: Microsoft Learn — Blazor Input Components]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

- Build output: net9.0, 0 warnings, 0 errors
- InsuranceFormModel: 9 fields with DataAnnotations, DateTime? for DateOfBirth, string for CoverageAmount/PolicyType
- EditForm with OnValidSubmit pattern (InteractiveServer, no FormName)
- InsuranceFormStateService registered as scoped in Program.cs
- FormSubmitted.razor placeholder created with redirect guard

### Completion Notes List

- Created InsuranceFormModel with all 9 fields: FirstName, LastName, DateOfBirth (DateTime?), Email ([EmailAddress]), PhoneNumber ([Phone]), Address, PolicyType (string with empty default), CoverageAmount (string), NomineeName
- All fields have [Required] and [Display] DataAnnotations
- Created InsuranceFormStateService scoped service for cross-page form data passing
- Registered InsuranceFormStateService in Program.cs alongside AuthService
- Created InsuranceForm.razor with @rendermode InteractiveServer, @attribute [Authorize], @page "/insurance-form"
- Bootstrap 5 card layout with two-column grid for professional appearance
- EditForm with DataAnnotationsValidator + ValidationSummary for client-side validation
- All 9 fields use Blazor input components: InputText, InputDate, InputSelect
- Stable id attributes on all inputs for Playwright automation (firstName, lastName, dateOfBirth, email, phoneNumber, address, policyType, coverageAmount, nomineeName, submitButton)
- PolicyType uses InputSelect with 4 options + placeholder (avoids enum validation bug)
- OnValidSubmit stores data in InsuranceFormStateService and navigates to /form-submitted
- Created placeholder FormSubmitted.razor with success message and redirect guard (full display in Story 2.2)
- No additional NuGet packages needed
- Build passes with 0 errors, 0 warnings

### Change Log

- 2026-02-13: Story 2.1 implemented — insurance form with 9 validated fields, state service, placeholder confirmation page

### File List

- BmadPro/Models/InsuranceFormModel.cs (created — 9 fields with DataAnnotations)
- BmadPro/Services/InsuranceFormStateService.cs (created — scoped state container for form data)
- BmadPro/Components/Pages/InsuranceForm.razor (created — InteractiveServer form with EditForm)
- BmadPro/Components/Pages/FormSubmitted.razor (created — placeholder confirmation page)
- BmadPro/Program.cs (modified — added InsuranceFormStateService registration)
