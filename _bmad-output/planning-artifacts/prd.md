---
stepsCompleted:
  - step-01-init
  - step-02-discovery
  - step-03-success
  - step-04-journeys
  - step-05-domain-skipped
  - step-06-innovation-skipped
  - step-07-project-type
  - step-08-scoping
  - step-09-functional
  - step-10-nonfunctional
  - step-11-polish
  - step-12-complete
inputDocuments: []
workflowType: 'prd'
documentCounts:
  briefs: 0
  research: 0
  brainstorming: 0
  projectDocs: 0
classification:
  projectType: web_app_automation_hybrid
  domain: general
  complexity: low
  projectContext: greenfield
---

# Product Requirements Document - BmadPro

**Author:** LNV-218
**Date:** 2026-02-13

## Executive Summary

BmadPro is a .NET 8 proof-of-concept demonstrating Elsa Workflow orchestrating Playwright browser automation for insurance form processing. The application combines a Blazor Server web app (Login, Insurance Form, Form Submitted pages) with an Elsa Workflow that drives Chromium in headed mode — auto-filling credentials, completing an insurance form, submitting it, and capturing a screenshot.

**Differentiator:** Showcases Elsa Workflow + Playwright as a viable, visual automation approach in a single .NET 8 solution — designed to impress clients in live demos.

**Target Audience:** Client demo for insurance operations stakeholders evaluating browser automation solutions.

**Tech Stack:** .NET 8, Blazor Server, Bootstrap 5, Elsa Workflow 3.x, Playwright for .NET, Chromium.

## Success Criteria

### User Success

- Client sees a polished, professional demo with clean UI across all 3 pages
- Elsa Workflow triggers Playwright to auto-fill login, fill the insurance form, submit, and capture a screenshot seamlessly
- Screenshot saved to `InsuranceScreenshot/` folder as proof of completion

### Business Success

- Demonstrates Elsa Workflow + Playwright + .NET 8 as a viable automation approach
- Client is impressed by UI quality and smooth end-to-end automation flow
- POC serves as foundation for pitching a larger automation solution

### Technical Success

- Built on .NET 8 with Blazor Server
- Elsa Workflow orchestrates automation steps in sequence
- Playwright drives Chromium browser automation (auto-fill + submit)
- Screenshot generated and stored in `InsuranceScreenshot/` folder
- All 3 pages render with modern, professional Bootstrap 5 styling

### Measurable Outcomes

- Login auto-fill completes without errors
- All 8 insurance form fields populated automatically
- Form submission succeeds and confirmation page displays
- Screenshot file exists in `InsuranceScreenshot/` after the run

## Product Scope

### MVP Strategy

**Approach:** Problem-Solving MVP — the simplest deliverable that proves Elsa Workflow can orchestrate Playwright to automate browser tasks reliably and visually.

**Resource Requirements:** Single .NET developer with Blazor, Elsa Workflow, and Playwright knowledge.

### MVP Feature Set (This POC)

- Login page with Bootstrap card layout, username/password fields, Login button
- Insurance Form page with 8 fields: Full Name, Date of Birth, Email, Phone Number, Address, Policy Type (dropdown), Coverage Amount, Nominee Name
- Form Submitted confirmation page with success message and submitted data summary
- Elsa Workflow orchestrating the full Playwright automation sequence
- Playwright automating Chromium in headed mode: login → form fill → submit → screenshot
- Screenshot saved to `InsuranceScreenshot/` folder in project root
- Hardcoded demo credentials and form data
- In-memory state (no database)

### Phase 2 (Growth)

- Configurable demo data (JSON/CSV input instead of hardcoded)
- Console/dashboard showing Elsa Workflow step execution in real-time
- Error handling with retry logic for flaky page loads
- Multiple insurance form templates (health, auto, life)
- Logging and execution reports

### Phase 3 (Expansion)

- Full RPA platform with drag-and-drop Elsa Workflow designer
- Multi-site automation support (beyond single insurance form)
- External data source integration (database, API)
- Batch automation for multiple form submissions
- Enterprise reporting dashboard

### Risk Mitigation

**Technical:** Elsa Workflow and Playwright are well-documented .NET libraries. Mitigation: use Elsa's built-in activity model and Playwright's auto-wait features.
**Market:** Minimal — client demo, not a market launch. Success = client is impressed.
**Resource:** Single developer can deliver. If time-constrained, confirmation page can be simplified to a basic success message.

## User Journeys

### Journey 1: Demo Presenter — Running the Automation (Success Path)

**Persona:** Raj, Solutions Engineer, preparing for a client demo.

**Opening Scene:** Raj opens the BmadPro solution in Visual Studio 2022. The client is on a screen share, curious about Elsa Workflow capabilities for insurance operations.

**Rising Action:** Raj hits F5. The Blazor app starts, and the Elsa Workflow kicks off automatically. A Chromium browser opens — the client watches in real-time as Playwright navigates to the Login page. Username and password fields fill themselves. The login button clicks.

**Climax:** The Insurance Form page loads with clean Bootstrap styling. Fields fill automatically one by one — Full Name, Date of Birth, Email, Phone Number, Address, Policy Type, Coverage Amount, Nominee Name. The client watches each field populate. Submit clicks. The confirmation page appears.

**Resolution:** Screenshot captured and saved to `InsuranceScreenshot/`. Raj shows the client the saved file. The client sees exactly how Elsa Workflow orchestrates browser automation end-to-end.

### Journey 2: Demo Presenter — Handling Delays (Edge Case)

**Persona:** Raj again, but the browser takes a moment to launch.

**Opening Scene:** Raj starts the project. The browser launch is slightly delayed.

**Rising Action:** Playwright waits for the page to load. Login fields fill correctly. The form page takes a bit longer to render.

**Climax:** Playwright's auto-wait mechanisms handle the delay gracefully — fields fill only after each element is ready. No crashes, no stale element errors.

**Resolution:** Automation completes successfully despite timing variation. Screenshot saved. The client sees resilient, production-quality automation.

### Journey 3: Client Viewer — Watching the Demo

**Persona:** Priya, Operations Manager at an insurance company evaluating automation tools.

**Opening Scene:** Priya joins a screen share. She's skeptical — she's seen automation demos fail before.

**Rising Action:** She watches the browser open, the login page appear with polished styling, and credentials fill automatically. The insurance form loads — professional, like a real internal tool.

**Climax:** Every form field populates smoothly: name, DOB, email, phone, address, policy type, coverage amount, nominee. The form submits. A clean confirmation page appears.

**Resolution:** Priya sees the screenshot saved to the project folder. She's convinced this approach could automate their repetitive data entry workflows. She asks about scaling it.

### Journey Requirements Summary

| Capability | Revealed By |
|---|---|
| Launch from Visual Studio / .NET 8 | Journey 1 |
| Elsa Workflow orchestrating sequential steps | Journey 1, 2 |
| Playwright browser automation (real-time, visible) | Journey 1, 3 |
| Professional Login page UI | Journey 1, 3 |
| Insurance Form with 8 fields | Journey 1, 3 |
| Form Submitted confirmation page | Journey 1, 3 |
| Screenshot capture to `InsuranceScreenshot/` | Journey 1, 3 |
| Graceful wait/retry handling | Journey 2 |
| Demo-worthy visual experience | Journey 3 |

## Technical Requirements

### Architecture Overview

A .NET 8 Blazor Server application with Bootstrap 5 styling, serving 3 pages. Elsa Workflow orchestrates Playwright browser automation against the locally running Blazor app, driving Chromium in headed mode for real-time client demo visibility.

### Tech Stack

- **Framework:** Blazor Server (.NET 8) — server-side rendering with SignalR
- **UI:** Bootstrap 5 (bundled with Blazor template)
- **Workflow Engine:** Elsa Workflow 3.x for .NET
- **Browser Automation:** Playwright for .NET — Chromium in headed mode
- **Pages:**
  1. **Login** — Username/Password fields, Login button, Bootstrap card layout
  2. **Insurance Form** — 8 fields (Full Name, DOB, Email, Phone, Address, Policy Type dropdown, Coverage Amount, Nominee Name), Submit button
  3. **Confirmation** — Success message with submitted data summary

### Blazor-Specific Requirements

- Blazor Server hosting model (no WASM needed for POC)
- In-memory authentication with hardcoded demo credentials
- Form validation via `EditForm` and `DataAnnotations`
- Page navigation via Blazor routing (`@page` directives)

### Elsa Workflow Integration

- Registered as a hosted service in the .NET 8 app
- Sequential workflow activities:
  1. Launch Chromium (headed mode)
  2. Navigate to Login page
  3. Fill username and password
  4. Click Login
  5. Wait for Insurance Form page
  6. Fill all form fields with demo data
  7. Click Submit
  8. Wait for confirmation page
  9. Take screenshot → save to `InsuranceScreenshot/`
  10. Close browser

### Implementation Constraints

- **Single Solution:** One .NET 8 project — Blazor web app + Elsa + Playwright automation
- **Launch:** F5 in Visual Studio starts Blazor, then Elsa Workflow triggers Playwright automatically
- **Demo Data:** Hardcoded realistic user data (no external data source)
- **No Database:** In-memory state only
- **Out of Scope:** SEO, accessibility compliance, responsive breakpoints, multi-browser support

## Functional Requirements

### Authentication

- **FR1:** User can enter a username on the Login page
- **FR2:** User can enter a password on the Login page
- **FR3:** User can submit login credentials to authenticate
- **FR4:** System validates login credentials against demo data
- **FR5:** System redirects authenticated user to the Insurance Form page

### Insurance Form

- **FR6:** User can enter their full name
- **FR7:** User can select their date of birth
- **FR8:** User can enter their email address
- **FR9:** User can enter their phone number
- **FR10:** User can enter their address
- **FR11:** User can select a policy type from predefined options
- **FR12:** User can enter a coverage amount
- **FR13:** User can enter a nominee name
- **FR14:** User can submit the completed insurance form

### Form Confirmation

- **FR15:** System displays a confirmation page after successful form submission
- **FR16:** System displays a summary of submitted form data on the confirmation page

### Workflow Orchestration

- **FR17:** Elsa Workflow launches a Chromium browser instance in headed (visible) mode
- **FR18:** Elsa Workflow navigates the browser to the Login page
- **FR19:** Elsa Workflow auto-fills login credentials via Playwright
- **FR20:** Elsa Workflow triggers login submission via Playwright
- **FR21:** Elsa Workflow auto-fills all insurance form fields via Playwright
- **FR22:** Elsa Workflow triggers form submission via Playwright
- **FR23:** Elsa Workflow waits for page elements before interacting with them

### Screenshot Capture

- **FR24:** System captures a full-page screenshot of the confirmation page
- **FR25:** System saves the screenshot to the `InsuranceScreenshot/` folder in the project directory
- **FR26:** System creates the `InsuranceScreenshot/` folder if it does not exist

## Non-Functional Requirements

### Performance

- **NFR1:** Blazor pages load within 2 seconds on localhost
- **NFR2:** Playwright field-fill actions have a visible delay (300-500ms between fields) for real-time observation
- **NFR3:** Full workflow execution (login → form fill → submit → screenshot) completes within 30 seconds

### Reliability

- **NFR4:** Playwright uses auto-wait for element readiness before interacting with any page element
- **NFR5:** Automation workflow completes successfully on repeated consecutive runs without manual intervention
- **NFR6:** Screenshot file written to disk before workflow reports completion

### Usability (Demo Experience)

- **NFR7:** All 3 pages have polished, professional appearance using Bootstrap 5
- **NFR8:** Browser automation runs in headed mode — all actions visible to viewer
- **NFR9:** Automation runs with a single action (F5 in Visual Studio) — no additional manual steps
