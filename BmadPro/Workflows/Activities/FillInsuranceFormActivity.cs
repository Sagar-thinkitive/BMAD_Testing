using Elsa.Extensions;
using Elsa.Workflows;

namespace BmadPro.Workflows.Activities;

public class FillInsuranceFormActivity : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var session = context.GetRequiredService<PlaywrightSession>();
        var config = context.GetRequiredService<IConfiguration>();
        var logger = context.GetRequiredService<ILogger<FillInsuranceFormActivity>>();

        try
        {
            logger.LogInformation("Filling insurance form fields...");

            var page = session.Page!;

            // Wait for Blazor InteractiveServer SignalR circuit to connect.
            // The page renders as static SSR first, then Blazor enhances it.
            // If we fill before the circuit connects, Blazor re-renders and wipes values.
            await page.WaitForFunctionAsync("() => window.Blazor !== undefined");
            await Task.Delay(1000);

            await page.Locator("#firstName").FillAsync(config["DemoData:InsuranceForm:FirstName"]!);
            await page.Locator("#lastName").FillAsync(config["DemoData:InsuranceForm:LastName"]!);
            await page.Locator("#dateOfBirth").FillAsync(config["DemoData:InsuranceForm:DateOfBirth"]!);
            await page.Locator("#email").FillAsync(config["DemoData:InsuranceForm:Email"]!);
            await page.Locator("#phoneNumber").FillAsync(config["DemoData:InsuranceForm:PhoneNumber"]!);

            // PolicyType is a <select> dropdown
            await page.Locator("#policyType").SelectOptionAsync(config["DemoData:InsuranceForm:PolicyType"]!);

            await page.Locator("#address").FillAsync(config["DemoData:InsuranceForm:Address"]!);
            await page.Locator("#coverageAmount").FillAsync(config["DemoData:InsuranceForm:CoverageAmount"]!);
            await page.Locator("#nomineeName").FillAsync(config["DemoData:InsuranceForm:NomineeName"]!);

            logger.LogInformation("Submitting insurance form...");
            await page.Locator("#submitButton").ClickAsync();
            await page.WaitForURLAsync("**/form-submitted");

            logger.LogInformation("Insurance form submitted — navigated to confirmation page.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Insurance form automation failed");
            throw;
        }
    }
}
