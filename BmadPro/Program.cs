using BmadPro.Components;
using BmadPro.Services;
using BmadPro.Workflows;
using Elsa.Extensions;
using Elsa.Workflows;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<InsuranceFormStateService>();
builder.Services.AddSingleton<PlaywrightSession>();

// Elsa Workflow
builder.Services.AddElsa(elsa =>
{
    elsa.AddActivitiesFrom<Program>();
    elsa.AddWorkflow<InsuranceAutomationWorkflow>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// CRITICAL ORDER: Authentication → Authorization → Antiforgery
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

// Login endpoint — validates credentials and sets auth cookie
app.MapPost("/api/auth/login", async (HttpContext context, AuthService authService) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();

    if (authService.ValidateCredentials(username, password))
    { 
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        context.Response.Redirect("/insurance-form");
        return;
    }

    context.Response.Redirect("/login?error=true");
}).DisableAntiforgery();

// Logout endpoint
app.MapGet("/api/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    context.Response.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Trigger workflow after app is fully started
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        var workflowRunner = scope.ServiceProvider.GetRequiredService<IWorkflowRunner>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Wait for Kestrel to be fully accepting requests
        await Task.Delay(TimeSpan.FromSeconds(3));

        logger.LogInformation("Starting insurance automation workflow...");
        try
        {
            var result = await workflowRunner.RunAsync<InsuranceAutomationWorkflow>();
            logger.LogInformation("Workflow completed with status: {Status}", result.WorkflowState.Status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Workflow execution failed");
        }
    });
});

app.Run();
