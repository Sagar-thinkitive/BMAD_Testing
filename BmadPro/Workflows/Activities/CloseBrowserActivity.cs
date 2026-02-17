using Elsa.Extensions;
using Elsa.Workflows;

namespace BmadPro.Workflows.Activities;

public class CloseBrowserActivity : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var session = context.GetRequiredService<PlaywrightSession>();
        var logger = context.GetRequiredService<ILogger<CloseBrowserActivity>>();

        logger.LogInformation("Closing browser...");
        await session.DisposeAsync();
        logger.LogInformation("Browser closed. Workflow complete.");
    }
}
