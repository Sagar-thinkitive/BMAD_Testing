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

        session.Playwright = await Playwright.CreateAsync();
        session.Browser = await session.Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 300 // NFR2: 300-500ms delay for demo visibility
        });

        var browserContext = await session.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true // Accept self-signed dev certificate on localhost
        });
        session.Page = await browserContext.NewPageAsync();

        logger.LogInformation("Browser launched successfully.");
    }
}
