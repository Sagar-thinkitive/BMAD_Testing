using Elsa.Extensions;
using Elsa.Workflows;

namespace BmadPro.Workflows.Activities;

public class FillLoginFormActivity : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var session = context.GetRequiredService<PlaywrightSession>();
        var config = context.GetRequiredService<IConfiguration>();
        var logger = context.GetRequiredService<ILogger<FillLoginFormActivity>>();

        try
        {
            var appUrl = config["AppUrl"]!;
            var username = config["DemoData:LoginCredentials:Username"]!;
            var password = config["DemoData:LoginCredentials:Password"]!;

            logger.LogInformation("Navigating to login page: {Url}", appUrl);
            await session.Page!.GotoAsync(appUrl);

            logger.LogInformation("Filling login credentials...");
            await session.Page.Locator("#username").FillAsync(username);
            await session.Page.Locator("#password").FillAsync(password);

            logger.LogInformation("Submitting login form...");
            await session.Page.Locator("button[type=\"submit\"]").ClickAsync();
            await session.Page.WaitForURLAsync("**/insurance-form");

            logger.LogInformation("Login successful — navigated to insurance form.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login automation failed");
            throw;
        }
    }
}
