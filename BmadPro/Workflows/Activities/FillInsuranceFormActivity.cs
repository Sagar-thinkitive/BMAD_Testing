using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Playwright;

namespace BmadPro.Workflows.Activities;

public class FillInsuranceFormActivity : CodeActivity
{
    private const int FieldDelayMs = 500; // NFR2: 300-500ms visible delay between field fills

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var session = context.GetRequiredService<PlaywrightSession>();
        var config = context.GetRequiredService<IConfiguration>();
        var logger = context.GetRequiredService<ILogger<FillInsuranceFormActivity>>();

        try
        {
            var page = session.Page!;

            // Wait for Blazor InteractiveServer SignalR circuit to connect.
            // The page renders as static SSR first, then Blazor enhances it via SignalR.
            // If we fill before the circuit connects, Blazor re-renders and WIPES all values.
            logger.LogInformation("Waiting for Blazor SignalR circuit to connect...");
            await page.WaitForFunctionAsync("() => window.Blazor !== undefined");
            await Task.Delay(2000); // Extra buffer for circuit to stabilize and EditForm to bind

            // Verify the form is interactive before filling
            await page.Locator("#firstName").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            logger.LogInformation("Insurance form is ready. Filling fields...");

            // Blazor InteractiveServer: InputText with @bind-Value listens for the 'change' event
            // through Blazor's event delegation (blazor.web.js → SignalR circuit).
            // Playwright's FillAsync sets the DOM value, then we dispatch input+change events
            // with bubbles:true so they reach blazor.web.js's document-level listener and
            // propagate through SignalR to update the server-side model.

            // Helper: Fill a text field and notify Blazor's binding system
            async Task FillBlazorField(string selector, string value, string fieldName)
            {
                var locator = page.Locator(selector);
                await locator.ClickAsync();
                await locator.FillAsync(value);

                // Dispatch bubbling events for Blazor's delegated event listener
                await page.EvaluateAsync(@"(sel) => {
                    const el = document.querySelector(sel);
                    if (!el) return;
                    el.dispatchEvent(new Event('input', { bubbles: true }));
                    el.dispatchEvent(new Event('change', { bubbles: true }));
                }", selector);

                await locator.PressAsync("Tab");
                await Task.Delay(FieldDelayMs);
                logger.LogInformation("Filled {Field}: '{Value}'", fieldName, value);
            }

            await FillBlazorField("#firstName", config["DemoData:InsuranceForm:FirstName"]!, "FirstName");
            await FillBlazorField("#lastName", config["DemoData:InsuranceForm:LastName"]!, "LastName");
            await FillBlazorField("#dateOfBirth", config["DemoData:InsuranceForm:DateOfBirth"]!, "DateOfBirth");
            await FillBlazorField("#email", config["DemoData:InsuranceForm:Email"]!, "Email");
            await FillBlazorField("#phoneNumber", config["DemoData:InsuranceForm:PhoneNumber"]!, "PhoneNumber");

            // PolicyType: InputSelect renders as <select> — SelectOptionAsync + JS event dispatch
            await page.Locator("#policyType").SelectOptionAsync(config["DemoData:InsuranceForm:PolicyType"]!);
            await page.EvaluateAsync(@"() => {
                const el = document.querySelector('#policyType');
                if (!el) return;
                el.dispatchEvent(new Event('change', { bubbles: true }));
            }");
            await page.Locator("#policyType").PressAsync("Tab");
            await Task.Delay(FieldDelayMs);
            logger.LogInformation("Filled PolicyType: '{Value}'", config["DemoData:InsuranceForm:PolicyType"]!);

            await FillBlazorField("#address", config["DemoData:InsuranceForm:Address"]!, "Address");
            await FillBlazorField("#coverageAmount", config["DemoData:InsuranceForm:CoverageAmount"]!, "CoverageAmount");
            await FillBlazorField("#nomineeName", config["DemoData:InsuranceForm:NomineeName"]!, "NomineeName");

            logger.LogInformation("All 9 fields filled. Submitting insurance form...");

            // Brief pause for Blazor to process the last field's change event through SignalR
            await Task.Delay(1000);

            await page.Locator("#submitButton").ClickAsync();
            logger.LogInformation("Submit button clicked. Waiting for confirmation page...");

            // Wait for the confirmation page's success heading to appear
            await page.Locator("h3.text-success").WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            logger.LogInformation("Insurance form submitted — confirmation page loaded.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Insurance form automation failed");
            throw;
        }
    }
}
