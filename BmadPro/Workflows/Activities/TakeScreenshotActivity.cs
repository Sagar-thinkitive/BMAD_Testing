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

            // Wait for Blazor circuit on confirmation page (InteractiveServer render mode)
            logger.LogInformation("Waiting for confirmation page to fully render...");
            await page.WaitForFunctionAsync("() => window.Blazor !== undefined");
            await Task.Delay(1500); // Buffer for Blazor circuit to stabilize and render data

            // Wait for the success heading — confirms data rendered (not the null/redirect case)
            await page.Locator("h3.text-success").WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Wait for the data table to be visible — confirms all 9 fields rendered
            await page.Locator("table.table-striped").WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            logger.LogInformation("Confirmation page loaded with form data. Taking screenshot...");

            // Ensure screenshot directory exists (FR26)
            var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "InsuranceScreenshot");
            Directory.CreateDirectory(screenshotDir);

            var screenshotPath = Path.Combine(screenshotDir, $"confirmation_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            // Capture full-page screenshot (FR24) and save as PNG (FR25)
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            // Verify file was written to disk before reporting completion (NFR6)
            if (!File.Exists(screenshotPath))
            {
                throw new FileNotFoundException("Screenshot file was not written to disk", screenshotPath);
            }

            var fileInfo = new FileInfo(screenshotPath);
            logger.LogInformation("Screenshot saved to: {Path} ({Size} bytes)", screenshotPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Screenshot capture failed");
            throw;
        }
    }
}
