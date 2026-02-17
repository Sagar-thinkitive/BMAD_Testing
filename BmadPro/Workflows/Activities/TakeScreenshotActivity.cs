using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.Playwright;

namespace BmadPro.Workflows.Activities;

public class TakeScreenshotActivity : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var session = context.GetRequiredService<PlaywrightSession>();
        var logger = context.GetRequiredService<ILogger<TakeScreenshotActivity>>();

        try
        {
            var page = session.Page!;

            // Wait for confirmation page and Blazor circuit to fully render data
            await page.Locator(".text-success").WaitForAsync();
            await page.WaitForFunctionAsync("() => window.Blazor !== undefined");
            await Task.Delay(1000);
            logger.LogInformation("Confirmation page loaded. Taking screenshot...");

            // Ensure screenshot directory exists
            var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "InsuranceScreenshot");
            Directory.CreateDirectory(screenshotDir);

            var screenshotPath = Path.Combine(screenshotDir, $"confirmation_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            logger.LogInformation("Screenshot saved to: {Path}", screenshotPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Screenshot capture failed");
            throw;
        }
    }
}
