using BmadPro.Workflows.Activities;
using Elsa.Workflows;
using Elsa.Workflows.Activities;

namespace BmadPro.Workflows;

public class InsuranceAutomationWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        builder.Root = new Sequence
        {
            Activities =
            {
                new LaunchBrowserActivity(),
                new FillLoginFormActivity(),
                new FillInsuranceFormActivity(),
                new TakeScreenshotActivity(),
                new CloseBrowserActivity()
            }
        };
    }
}
