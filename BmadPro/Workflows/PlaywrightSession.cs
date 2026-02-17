using Microsoft.Playwright;

namespace BmadPro.Workflows;

public class PlaywrightSession : IAsyncDisposable
{
    public IPlaywright? Playwright { get; set; }
    public IBrowser? Browser { get; set; }
    public IPage? Page { get; set; }

    public async ValueTask DisposeAsync()
    {
        if (Page != null) { await Page.CloseAsync(); Page = null; }
        if (Browser != null) { await Browser.CloseAsync(); Browser = null; }
        Playwright?.Dispose();
        Playwright = null;
    }
}
