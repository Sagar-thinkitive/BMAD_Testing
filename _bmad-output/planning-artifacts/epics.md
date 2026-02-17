---
stepsCompleted: [1, 2, 3, 4]
lastStep: 4
status: 'complete'
completedAt: '2026-02-13'
inputDocuments:
  - planning-artifacts/prd.md
  - planning-artifacts/architecture.md
workflowType: 'epics'
project_name: 'BmadPro'
user_name: 'LNV-218'
date: '2026-02-13'
---

# BmadPro - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for BmadPro, decomposing the requirements from the PRD and Architecture into implementable stories.

## Requirements Inventory

### Functional Requirements

**Authentication (FR1-FR5):**
- FR1: User can enter a username on the Login page
- FR2: User can enter a password on the Login page
- FR3: User can submit login credentials to authenticate
- FR4: System validates login credentials against demo data
- FR5: System redirects authenticated user to the Insurance Form page

**Insurance Form (FR6-FR14):**
- FR6: User can enter their first name and last name (separate fields)
- FR7: User can select their date of birth
- FR8: User can enter their email address
- FR9: User can enter their phone number
- FR10: User can enter their address
- FR11: User can select a policy type from predefined options
- FR12: User can enter a coverage amount
- FR13: User can enter a nominee name
- FR14: User can submit the completed insurance form

**Form Confirmation (FR15-FR16):**
- FR15: System displays a confirmation page after successful form submission
- FR16: System displays a summary of submitted form data on the confirmation page

**Workflow Orchestration (FR17-FR23):**
- FR17: Elsa Workflow launches a Chromium browser instance in headed (visible) mode
- FR18: Elsa Workflow navigates the browser to the Login page
- FR19: Elsa Workflow auto-fills login credentials via Playwright
- FR20: Elsa Workflow triggers login submission via Playwright
- FR21: Elsa Workflow auto-fills all insurance form fields via Playwright
- FR22: Elsa Workflow triggers form submission via Playwright
- FR23: Elsa Workflow waits for page elements before interacting with them

**Screenshot Capture (FR24-FR26):**
- FR24: System captures a full-page screenshot of the confirmation page
- FR25: System saves the screenshot to the `InsuranceScreenshot/` folder in the project directory
- FR26: System creates the `InsuranceScreenshot/` folder if it does not exist

### NonFunctional Requirements

**Performance (NFR1-NFR3):**
- NFR1: Blazor pages load within 2 seconds on localhost
- NFR2: Playwright field-fill actions have a visible delay (300-500ms between fields) for real-time observation
- NFR3: Full workflow execution (login → form fill → submit → screenshot) completes within 30 seconds

**Reliability (NFR4-NFR6):**
- NFR4: Playwright uses auto-wait for element readiness before interacting with any page element
- NFR5: Automation workflow completes successfully on repeated consecutive runs without manual intervention
- NFR6: Screenshot file written to disk before workflow reports completion

**Usability / Demo Experience (NFR7-NFR9):**
- NFR7: All 3 pages have polished, professional appearance using Bootstrap 5
- NFR8: Browser automation runs in headed mode — all actions visible to viewer
- NFR9: Automation runs with a single action (F5 in Visual Studio) — no additional manual steps

### Additional Requirements

**From Architecture — Starter Template (impacts Epic 1, Story 1):**
- Initialize project using `dotnet new blazor --interactivity Server -n BmadPro`
- Add NuGet packages: Elsa 3.5.3, Microsoft.Playwright 1.58.0

**From Architecture — Authentication:**
- ASP.NET Core Cookie Authentication with built-in `AuthenticationStateProvider`
- Credentials stored in `appsettings.json` `DemoData.LoginCredentials` section
- `[Authorize]` attribute on Insurance Form and Confirmation pages

**From Architecture — Data & Configuration:**
- All demo data in `appsettings.json` under `DemoData` section — never hardcode in C# files
- `FirstName` + `LastName` as separate fields (not FullName)
- Form model uses `DataAnnotations` validation attributes
- `EditForm` + `DataAnnotationsValidator` + `ValidationSummary` for form handling

**From Architecture — Startup & Sequencing:**
- `IHostApplicationLifetime.ApplicationStarted` event triggers Elsa Workflow after Blazor is fully ready
- Base URL read from `appsettings.json` or `IServer.Features`

**From Architecture — Project Structure:**
- Pages in `/Components/Pages/`: `Login.razor`, `InsuranceForm.razor`, `FormSubmitted.razor`
- Models in `/Models/`: `LoginModel.cs`, `InsuranceFormModel.cs`
- Services in `/Services/`: `AuthService.cs`
- Workflows in `/Workflows/`: `InsuranceAutomationWorkflow.cs`
- Activities in `/Workflows/Activities/`: `FillLoginFormActivity.cs`, `FillInsuranceFormActivity.cs`, `TakeScreenshotActivity.cs`

**From Architecture — Error Handling & Patterns:**
- `try/catch` in Playwright activities with `Console.WriteLine` logging
- Playwright auto-wait (no `Thread.Sleep()`)
- Playwright install: `pwsh bin/Debug/net8.0/playwright.ps1 install chromium`

### FR Coverage Map

| FR | Epic | Description |
|---|---|---|
| FR1 | Epic 1 | Username field on Login page |
| FR2 | Epic 1 | Password field on Login page |
| FR3 | Epic 1 | Submit login credentials |
| FR4 | Epic 1 | Validate credentials against demo data |
| FR5 | Epic 1 | Redirect to Insurance Form after auth |
| FR6 | Epic 2 | Enter first name and last name |
| FR7 | Epic 2 | Select date of birth |
| FR8 | Epic 2 | Enter email address |
| FR9 | Epic 2 | Enter phone number |
| FR10 | Epic 2 | Enter address |
| FR11 | Epic 2 | Select policy type from dropdown |
| FR12 | Epic 2 | Enter coverage amount |
| FR13 | Epic 2 | Enter nominee name |
| FR14 | Epic 2 | Submit completed insurance form |
| FR15 | Epic 2 | Display confirmation page after submission |
| FR16 | Epic 2 | Display summary of submitted data |
| FR17 | Epic 3 | Launch Chromium in headed mode |
| FR18 | Epic 3 | Navigate browser to Login page |
| FR19 | Epic 3 | Auto-fill login credentials via Playwright |
| FR20 | Epic 3 | Trigger login submission via Playwright |
| FR21 | Epic 3 | Auto-fill all insurance form fields |
| FR22 | Epic 3 | Trigger form submission via Playwright |
| FR23 | Epic 3 | Wait for page elements before interaction |
| FR24 | Epic 3 | Capture full-page screenshot |
| FR25 | Epic 3 | Save screenshot to `InsuranceScreenshot/` |
| FR26 | Epic 3 | Create `InsuranceScreenshot/` folder if needed |

## Epic List

### Epic 1: Project Setup & User Authentication
User can launch the Blazor app and successfully log in with demo credentials, seeing a professional Bootstrap 5 login page.
**FRs covered:** FR1, FR2, FR3, FR4, FR5
**NFRs addressed:** NFR1, NFR7

### Epic 2: Insurance Form & Confirmation
Authenticated user can fill out a 9-field insurance form with validation and submit it, landing on a professional confirmation page showing all submitted data.
**FRs covered:** FR6, FR7, FR8, FR9, FR10, FR11, FR12, FR13, FR14, FR15, FR16
**NFRs addressed:** NFR1, NFR7

### Epic 3: Elsa Workflow Automation & Screenshot
A single F5 press launches the entire automated demo — Elsa Workflow drives Playwright through login, form fill, submission, and screenshot capture, all visible in a headed Chromium browser.
**FRs covered:** FR17, FR18, FR19, FR20, FR21, FR22, FR23, FR24, FR25, FR26
**NFRs addressed:** NFR2, NFR3, NFR4, NFR5, NFR6, NFR8, NFR9

## Epic 1: Project Setup & User Authentication

User can launch the Blazor app and successfully log in with demo credentials, seeing a professional Bootstrap 5 login page.

### Story 1.1: Initialize Blazor Project with Dependencies

As a developer,
I want a scaffolded BmadPro Blazor Server project with all required NuGet packages and configuration structure,
So that I have a working foundation for building all application features.

**Acceptance Criteria:**

**Given** no existing project
**When** the project is initialized using `dotnet new blazor --interactivity Server -n BmadPro`
**Then** a Blazor Server project targeting .NET 8 is created with Bootstrap 5 included
**And** Elsa 3.5.3 and Microsoft.Playwright 1.58.0 NuGet packages are added
**And** `appsettings.json` contains the `AppUrl` and `DemoData` configuration structure (LoginCredentials + InsuranceForm sections)
**And** project directories are created: `Models/`, `Services/`, `Workflows/Activities/`
**And** `dotnet build` completes without errors

### Story 1.2: Implement Login Page with Cookie Authentication

As a demo presenter,
I want a professional Bootstrap 5 login page that authenticates against demo credentials and redirects to the insurance form,
So that the demo starts with a polished, secure authentication experience.

**Acceptance Criteria:**

**Given** the Blazor app is running
**When** a user navigates to the root URL
**Then** the `Login.razor` page displays with a Bootstrap 5 card layout containing username and password fields and a Login button (FR1, FR2)

**Given** valid demo credentials (from `appsettings.json` DemoData.LoginCredentials)
**When** the user enters username and password and clicks Login (FR3)
**Then** `AuthService` validates credentials against `appsettings.json` (FR4)
**And** a cookie authentication session is created
**And** the user is redirected to the Insurance Form page (FR5)

**Given** invalid credentials
**When** the user submits the login form
**Then** an error message displays on the login page
**And** no authentication cookie is set

**Given** an unauthenticated user
**When** they attempt to access `/insurance-form` or `/form-submitted` directly
**Then** they are redirected to the Login page

## Epic 2: Insurance Form & Confirmation

Authenticated user can fill out a 9-field insurance form with validation and submit it, landing on a professional confirmation page showing all submitted data.

### Story 2.1: Implement Insurance Form Page with Validation

As a demo presenter,
I want a professional Bootstrap 5 insurance form with 9 validated fields,
So that the demo showcases a realistic data-entry experience.

**Acceptance Criteria:**

**Given** an authenticated user on the Insurance Form page
**When** the page renders
**Then** `InsuranceForm.razor` displays a Bootstrap 5 styled form with these fields: FirstName (text), LastName (text), DateOfBirth (date), Email (email), PhoneNumber (text), Address (text), PolicyType (dropdown with predefined options), CoverageAmount (text), NomineeName (text) (FR6-FR13)
**And** the page is protected with `[Authorize]` attribute

**Given** the form is displayed
**When** the user leaves required fields empty and clicks Submit
**Then** `DataAnnotationsValidator` displays validation errors via `ValidationSummary`
**And** the form is not submitted

**Given** all fields are filled with valid data
**When** the user clicks Submit (FR14)
**Then** the form data is captured in `InsuranceFormModel` and the user is navigated to the confirmation page

### Story 2.2: Implement Form Submitted Confirmation Page

As a demo presenter,
I want a professional confirmation page that displays all submitted form data,
So that the demo ends with clear proof of successful form processing.

**Acceptance Criteria:**

**Given** a user has successfully submitted the insurance form
**When** the `FormSubmitted.razor` page renders (FR15)
**Then** a Bootstrap 5 styled success message is displayed
**And** a summary of all 9 submitted fields is shown: FirstName, LastName, DateOfBirth, Email, PhoneNumber, Address, PolicyType, CoverageAmount, NomineeName (FR16)

**Given** the confirmation page is displayed
**When** the user views the page
**Then** all field values match exactly what was submitted on the form

**Given** an unauthenticated user
**When** they navigate to `/form-submitted` directly
**Then** they are redirected to the Login page

## Epic 3: Elsa Workflow Automation & Screenshot

A single F5 press launches the entire automated demo — Elsa Workflow drives Playwright through login, form fill, submission, and screenshot capture, all visible in a headed Chromium browser.

### Story 3.1: Register Elsa Workflow with Startup Sequencing

As a demo presenter,
I want Elsa Workflow to automatically trigger after the Blazor app is fully started,
So that the automation begins seamlessly with a single F5 press.

**Acceptance Criteria:**

**Given** the BmadPro application starts via F5 or `dotnet run`
**When** the Blazor app is fully initialized and listening
**Then** `IHostApplicationLifetime.ApplicationStarted` event fires and triggers the `InsuranceAutomationWorkflow` (NFR9)
**And** Elsa Workflow is registered as an `IHostedService` in `Program.cs`
**And** the workflow reads the base URL from `appsettings.json` `AppUrl` setting

**Given** the workflow is triggered
**When** it begins execution
**Then** Playwright launches Chromium in headed (visible) mode (FR17, NFR8)
**And** the browser window is visible to the demo viewer

### Story 3.2: Implement Login Automation Activity

As a demo presenter,
I want Playwright to automatically fill and submit the login form,
So that the client watches credentials being entered in real-time.

**Acceptance Criteria:**

**Given** the Chromium browser is launched and the workflow is running
**When** `FillLoginFormActivity` executes
**Then** Playwright navigates to the Login page URL (FR18)
**And** waits for the login form elements to be ready (FR23, NFR4)
**And** fills the username field with demo credentials from `appsettings.json` (FR19)
**And** fills the password field with demo credentials from `appsettings.json` (FR19)
**And** a 300-500ms delay is applied between field fills for visual effect (NFR2)
**And** clicks the Login button (FR20)
**And** waits for navigation to the Insurance Form page

**Given** an error occurs during login automation
**When** the activity catches the exception
**Then** the error is logged via `Console.WriteLine`

### Story 3.3: Implement Insurance Form Automation Activity

As a demo presenter,
I want Playwright to automatically fill all 9 insurance form fields and submit,
So that the client watches the form being completed in real-time.

**Acceptance Criteria:**

**Given** the browser is on the Insurance Form page after successful login
**When** `FillInsuranceFormActivity` executes
**Then** Playwright waits for the form elements to be ready (FR23, NFR4)
**And** fills all 9 fields with demo data from `appsettings.json`: FirstName, LastName, DateOfBirth, Email, PhoneNumber, Address, PolicyType (selects from dropdown), CoverageAmount, NomineeName (FR21)
**And** a 300-500ms delay is applied between each field fill for visual effect (NFR2)
**And** clicks the Submit button (FR22)
**And** waits for navigation to the confirmation page

**Given** an error occurs during form automation
**When** the activity catches the exception
**Then** the error is logged via `Console.WriteLine`

### Story 3.4: Implement Screenshot Capture Activity

As a demo presenter,
I want a screenshot of the confirmation page saved automatically,
So that there is proof of successful end-to-end automation.

**Acceptance Criteria:**

**Given** the browser is on the FormSubmitted confirmation page
**When** `TakeScreenshotActivity` executes
**Then** a full-page screenshot is captured (FR24)
**And** the `InsuranceScreenshot/` directory is created if it does not exist (FR26)
**And** the screenshot is saved as a PNG file in `InsuranceScreenshot/` (FR25)
**And** the file is fully written to disk before the activity reports completion (NFR6)

**Given** the screenshot is saved
**When** the workflow completes
**Then** the browser is closed
**And** the full workflow execution completes within 30 seconds (NFR3)
**And** the workflow can be run repeatedly without manual intervention (NFR5)
