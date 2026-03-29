using Spectre.Console;
using Wisdom.Simulator.Helpers;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Scenarios;

/// <summary>
/// Scenario 2: Runs Scenario 1, then a customer registers, logs in, and purchases the course.
/// </summary>
public static class Scenario2
{
    public static async Task RunAsync(SimulatorContext ctx, CancellationToken ct)
    {
        // First run Scenario 1 to set up the course
        await Scenario1.RunAsync(ctx, ct);

        AnsiConsole.MarkupLine("");
        AnsiConsole.Write(new Rule("[bold cyan]Scenario 2 — Course Purchase[/]").RuleStyle("cyan"));
        AnsiConsole.MarkupLine("");

        await RegisterAndPurchase(ctx, ct);
    }

    /// <summary>
    /// Registers a single customer and purchases the current course in context.
    /// Returns the customer credentials and token for reuse in Scenario 3.
    /// </summary>
    public static async Task<(RegisterUserRequest Credentials, LoginUserResponse Token)> RegisterAndPurchase(
        SimulatorContext ctx, CancellationToken ct)
    {
        var customerReq = DataGenerator.FakeCustomer();

        // ── Register ─────────────────────────────────────────────────────────
        AnsiConsole.MarkupLine($"[grey]→[/] Registering customer [cyan]{customerReq.Email}[/]...");
        var registered = await ctx.UserClient.RegisterAsync(customerReq, ct);
        AnsiConsole.MarkupLine($"  [green]✓[/] Customer registered: [bold]{registered!.Name}[/] (ID: {registered.UserId})");

        // ── Login ─────────────────────────────────────────────────────────────
        AnsiConsole.MarkupLine($"[grey]→[/] Logging in...");
        var loginResp = await ctx.UserClient.LoginAsync(new LoginUserRequest(customerReq.Email, customerReq.Password), ct);
        AnsiConsole.MarkupLine($"  [green]✓[/] Token obtained");

        // ── Purchase ──────────────────────────────────────────────────────────
        ctx.OrderClient.SetBearerToken(loginResp!.AccessToken);
        AnsiConsole.MarkupLine($"[grey]→[/] Purchasing course [italic]\"{Markup.Escape(ctx.Course!.Title)}\"[/]...");
        var purchase = await ctx.OrderClient.PurchaseAsync(
            new PurchaseCourseRequest(ctx.Course.CourseId, ctx.Course.Price), ct);
        AnsiConsole.MarkupLine($"  [green]✓[/] Purchase complete! (PurchaseID: {purchase!.PurchaseId})");

        var entry = (customerReq, loginResp);
        ctx.Customers.Add(entry);
        return entry;
    }
}
