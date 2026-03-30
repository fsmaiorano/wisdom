using Spectre.Console;
using Wisdom.Simulator.Helpers;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Scenarios;

/// <summary>
/// Scenario 4: Like Scenario 3 but with randomised learning progression.
/// Each customer drops off at a different point — some never start, some finish,
/// most stop somewhere in the middle.
/// </summary>
public static class Scenario4
{
    private record CustomerRun(
        int Index,
        string Email,
        string Token,
        int LessonsToWatch,   // 0 = never starts, lessons.Count = finishes
        bool FinishesLastLesson);

    public static async Task RunAsync(SimulatorContext ctx, CancellationToken ct)
    {
        await Scenario1.RunAsync(ctx, ct);

        AnsiConsole.MarkupLine("");
        AnsiConsole.Write(new Rule("[bold blue]Scenario 4 — Randomised Learning Progression[/]").RuleStyle("blue"));

        int count = AnsiConsole.Ask<int>(
            $"[yellow]How many customers?[/] [grey](default: {ctx.Settings.NumberOfCustomers})[/]",
            ctx.Settings.NumberOfCustomers);

        AnsiConsole.MarkupLine($"\n[grey]Registering {count} customer(s) and purchasing the course...[/]\n");

        // Register & purchase
        var enrolled = new List<(RegisterUserRequest Credentials, LoginUserResponse Token)>();
        for (int i = 0; i < count; i++)
        {
            AnsiConsole.Write(new Rule($"[cyan]Customer {i + 1}/{count}[/]").RuleStyle("grey").LeftJustified());
            var entry = await Scenario2.RegisterAndPurchase(ctx, ct);
            enrolled.Add(entry);
            AnsiConsole.MarkupLine("");
        }

        // Assign a random dropout profile to each customer
        var lessons = ctx.Lessons.OrderBy(l => l.Position).ToList();
        var profiles = AssignProfiles(enrolled, lessons.Count);

        PrintProfileTable(profiles, lessons.Count);

        AnsiConsole.MarkupLine($"\n[bold]Starting watch simulation…[/] [grey]Ctrl+C to stop.[/]\n");

        var tasks = profiles
            .Where(p => p.LessonsToWatch > 0)
            .Select(p => WatchLessonsAsync(ctx, p, lessons, ct));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("\n[yellow]⚠ Simulation cancelled.[/]");
        }

        AnsiConsole.MarkupLine("");
        PrintResultTable(profiles, lessons.Count);
    }

    // ── Profile assignment ───────────────────────────────────────────────────

    private static List<CustomerRun> AssignProfiles(
        List<(RegisterUserRequest Credentials, LoginUserResponse Token)> enrolled, int totalLessons)
    {
        var rng = Random.Shared;
        var profiles = new List<CustomerRun>();

        for (int i = 0; i < enrolled.Count; i++)
        {
            var (creds, token) = enrolled[i];

            // Weighted bucket:  15% never start | 65% partial dropout | 20% finisher
            double roll = rng.NextDouble();
            int lessonsToWatch;
            bool finishesLast;

            if (roll < 0.15)
            {
                // Never starts
                lessonsToWatch = 0;
                finishesLast = false;
            }
            else if (roll < 0.80)
            {
                // Drops off after a random lesson (1 … totalLessons-1 when possible, else 1 … totalLessons)
                lessonsToWatch = totalLessons <= 1
                    ? 1
                    : rng.Next(1, totalLessons); // exclusive upper bound → never reaches totalLessons here
                finishesLast = rng.NextDouble() < 0.40; // 40% chance they complete their last lesson before quitting
            }
            else
            {
                // Finisher
                lessonsToWatch = totalLessons;
                finishesLast = true;
            }

            profiles.Add(new CustomerRun(i + 1, creds.Email, token.AccessToken, lessonsToWatch, finishesLast));
        }

        return profiles;
    }

    // ── Watch simulation ─────────────────────────────────────────────────────

    private static async Task WatchLessonsAsync(
        SimulatorContext ctx,
        CustomerRun profile,
        List<GetLessonResponse> lessons,
        CancellationToken ct)
    {
        var client = new Clients.LearningApiClient(ctx.Settings.BaseUrl);
        client.SetBearerToken(profile.Token);

        for (int i = 0; i < profile.LessonsToWatch; i++)
        {
            ct.ThrowIfCancellationRequested();

            var lesson = lessons[i];
            bool isLastWatched = i == profile.LessonsToWatch - 1;
            bool isCompleted = !isLastWatched || profile.FinishesLastLesson;

            int seconds = isCompleted
                ? DataGenerator.FakeWatchedSeconds(maxSeconds: 600)
                : DataGenerator.FakeWatchedSeconds(maxSeconds: 120); // partial — short watch

            AnsiConsole.MarkupLine(
                $"  [blue]Customer {profile.Index}[/] ([grey]{profile.Email}[/]) " +
                $"watching [italic]\"{Markup.Escape(lesson.Title)}\"[/] " +
                $"[grey]({seconds}s, {(isCompleted ? "completed" : "partial")})[/]...");

            await Task.Delay(ctx.Settings.WatchDelayMs, ct);

            await client.UpdateProgressAsync(
                new UpdateProgressRequest(lesson.LessonId, seconds, isCompleted), ct);
        }

        var outcome = profile.LessonsToWatch == lessons.Count
            ? "[bold green]finished[/]"
            : $"[yellow]dropped at lesson {profile.LessonsToWatch}/{lessons.Count}[/]";
        AnsiConsole.MarkupLine($"  [blue]Customer {profile.Index}[/] → {outcome}");
    }

    // ── Console output ───────────────────────────────────────────────────────

    private static void PrintProfileTable(List<CustomerRun> profiles, int totalLessons)
    {
        var table = new Table().Border(TableBorder.Simple).BorderColor(Color.Blue);
        table.AddColumn("[blue]#[/]");
        table.AddColumn("[blue]Email[/]");
        table.AddColumn("[blue]Assigned progression[/]");

        foreach (var p in profiles)
        {
            string plan = p.LessonsToWatch == 0
                ? "[grey]never starts[/]"
                : p.LessonsToWatch == totalLessons
                    ? "[green]finishes course[/]"
                    : $"[yellow]watches {p.LessonsToWatch}/{totalLessons} lessons[/]";
            table.AddRow(p.Index.ToString(), p.Email, plan);
        }

        AnsiConsole.Write(table);
    }

    private static void PrintResultTable(List<CustomerRun> profiles, int totalLessons)
    {
        int neverStarted = profiles.Count(p => p.LessonsToWatch == 0);
        int finished = profiles.Count(p => p.LessonsToWatch == totalLessons);
        int partial = profiles.Count - neverStarted - finished;

        var summary = new Table().Border(TableBorder.Rounded).BorderColor(Color.Blue);
        summary.AddColumn("[blue]Outcome[/]");
        summary.AddColumn("[blue]Count[/]");
        summary.AddRow("[grey]Never started[/]", neverStarted.ToString());
        summary.AddRow("[yellow]Partial (dropped)[/]", partial.ToString());
        summary.AddRow("[green]Finished course[/]", finished.ToString());
        summary.AddRow("[bold]Total[/]", profiles.Count.ToString());
        AnsiConsole.Write(summary);
    }
}
