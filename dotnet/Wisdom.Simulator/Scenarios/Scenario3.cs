using Spectre.Console;
using Wisdom.Simulator.Helpers;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Scenarios;

/// <summary>
/// Scenario 3: Runs Scenario 1 to create the course, then X customers register, purchase,
/// and watch lessons one by one. Keeps running until all customers finish or Ctrl+C is pressed.
/// </summary>
public static class Scenario3
{
    public static async Task RunAsync(SimulatorContext ctx, CancellationToken ct)
    {
        // Set up the course
        await Scenario1.RunAsync(ctx, ct);

        AnsiConsole.MarkupLine("");
        AnsiConsole.Write(new Rule("[bold magenta]Scenario 3 — Watch Lessons Loop[/]").RuleStyle("magenta"));

        int count = AnsiConsole.Ask<int>(
            $"[yellow]How many customers should buy and watch the course?[/] [grey](default: {ctx.Settings.NumberOfCustomers})[/]",
            ctx.Settings.NumberOfCustomers);

        AnsiConsole.MarkupLine($"\n[grey]Registering {count} customer(s) and purchasing the course...[/]\n");

        // Register all customers and purchase
        var customers = new List<(RegisterUserRequest Credentials, LoginUserResponse Token)>();
        for (int i = 0; i < count; i++)
        {
            AnsiConsole.Write(new Rule($"[cyan]Customer {i + 1}/{count}[/]").RuleStyle("grey").LeftJustified());
            var entry = await Scenario2.RegisterAndPurchase(ctx, ct);
            customers.Add(entry);
            AnsiConsole.MarkupLine("");
        }

        AnsiConsole.MarkupLine($"\n[bold]All {count} customer(s) enrolled.[/] Starting lesson-by-lesson watch loop...");
        AnsiConsole.MarkupLine("[grey]Press Ctrl+C to stop at any time.[/]\n");

        var lessons = ctx.Lessons;

        // Each customer watches lessons sequentially, run in parallel
        var watchTasks = customers.Select((customer, idx) =>
            WatchAllLessonsAsync(ctx, customer.Credentials.Email, customer.Token.AccessToken, idx + 1, lessons, ct));

        try
        {
            await Task.WhenAll(watchTasks);
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("\n[yellow]⚠ Watch loop cancelled.[/]");
        }

        AnsiConsole.MarkupLine("\n[bold green]✓ All customers have finished watching all lessons![/]");
    }

    private static async Task WatchAllLessonsAsync(
        SimulatorContext ctx,
        string customerEmail,
        string token,
        int customerIndex,
        List<GetLessonResponse> lessons,
        CancellationToken ct)
    {
        var learningClient = new Clients.LearningApiClient(ctx.Settings.BaseUrl);
        learningClient.SetBearerToken(token);

        foreach (var lesson in lessons.OrderBy(l => l.Position))
        {
            ct.ThrowIfCancellationRequested();

            var watchedSeconds = DataGenerator.FakeWatchedSeconds();

            AnsiConsole.MarkupLine(
                $"  [magenta]Customer {customerIndex}[/] ([grey]{customerEmail}[/]) " +
                $"watching [italic]\"{Markup.Escape(lesson.Title)}\"[/] " +
                $"[grey]({watchedSeconds}s)[/]...");

            await Task.Delay(ctx.Settings.WatchDelayMs, ct);

            var progress = await learningClient.UpdateProgressAsync(
                new UpdateProgressRequest(lesson.LessonId, watchedSeconds, IsCompleted: true), ct);

            AnsiConsole.MarkupLine(
                $"  [magenta]Customer {customerIndex}[/] [green]✓[/] completed lesson " +
                $"[bold]{lesson.Position}/{lessons.Count}[/] " +
                $"(ProgressID: {progress!.ProgressId})");
        }

        AnsiConsole.MarkupLine(
            $"\n  [bold green]Customer {customerIndex} ({customerEmail}) finished all {lessons.Count} lessons! 🎉[/]\n");
    }
}
