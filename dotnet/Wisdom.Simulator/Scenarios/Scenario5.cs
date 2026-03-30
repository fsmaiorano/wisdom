using Spectre.Console;
using Wisdom.Simulator.Clients;
using Wisdom.Simulator.Config;
using Wisdom.Simulator.Helpers;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Scenarios;

/// <summary>
/// Scenario 5: Like Scenario 4 but every call goes through the BFF (http://localhost:5000).
/// After the watch simulation, it reads the BFF aggregation endpoints — Dashboard and CoursePage —
/// to show the consolidated view each customer would see in the frontend.
/// </summary>
public static class Scenario5
{
    private record CustomerRun(
        int Index,
        string Email,
        string Token,
        int LessonsToWatch,
        bool FinishesLastLesson);

    public static async Task RunAsync(SimulatorSettings settings, CancellationToken ct)
    {
        var bff = new BffApiClient(settings.BaseUrl);

        AnsiConsole.Write(new Rule("[bold magenta]Scenario 5 — Full Flow via BFF[/]").RuleStyle("magenta"));
        AnsiConsole.MarkupLine($"\n[grey]All calls routed through the BFF at [bold]{settings.BaseUrl}[/][/]\n");

        // ── Step 1: Teacher creates course + lessons ─────────────────────────
        var teacherReq = DataGenerator.FakeTeacher();
        AnsiConsole.MarkupLine($"[grey]→[/] Registering teacher [cyan]{teacherReq.Email}[/]...");
        await bff.RegisterAsync(teacherReq, ct);

        AnsiConsole.MarkupLine("[grey]→[/] Logging in as teacher...");
        var teacherLogin = await bff.LoginAsync(new LoginUserRequest(teacherReq.Email, teacherReq.Password), ct);
        bff.SetBearerToken(teacherLogin!.AccessToken);
        AnsiConsole.MarkupLine("  [green]✓[/] Teacher token obtained");

        AnsiConsole.MarkupLine("[grey]→[/] Creating course...");
        var courseReq = DataGenerator.FakeCourse();
        var course = await bff.CreateCourseAsync(courseReq, ct);
        AnsiConsole.MarkupLine($"  [green]✓[/] Course: [bold]{Markup.Escape(course!.Title)}[/] (ID: {course.CourseId})");

        var mockFiles = DataGenerator.FindMockVideos();
        if (mockFiles.Length > 0)
            AnsiConsole.MarkupLine($"[grey]→[/] Found [bold]{mockFiles.Length}[/] mock video(s) — will cycle through them per lesson");
        else
            AnsiConsole.MarkupLine("[yellow]⚠[/]  No .mkv files found in mocks/ — lessons will be created without video");

        var lessons = new List<GetLessonResponse>();
        AnsiConsole.MarkupLine($"[grey]→[/] Adding {settings.LessonsPerCourse} lessons...");
        for (int i = 1; i <= settings.LessonsPerCourse; i++)
        {
            var lessonReq = DataGenerator.FakeLesson(i);
            var videoPath = mockFiles.Length > 0 ? mockFiles[(i - 1) % mockFiles.Length] : null;
            var thumbnailUrl = DataGenerator.FakeThumbnailUrl();

            var lesson = await bff.CreateLessonAsync(course.CourseId, lessonReq, videoPath, thumbnailUrl, ct);
            lessons.Add(lesson!);

            var mediaTag = lesson!.MediaId.HasValue ? $" [blue]▶ media:{lesson.MediaId}[/]" : " [grey](no video)[/]";
            AnsiConsole.MarkupLine($"    [green]+[/] [[{i}/{settings.LessonsPerCourse}]] [italic]{Markup.Escape(lesson.Title)}[/]{mediaTag}");
        }

        AnsiConsole.MarkupLine("[grey]→[/] Publishing course...");
        await bff.PublishCourseAsync(course.CourseId, true, ct);
        AnsiConsole.MarkupLine("  [green]✓[/] Course published");
        bff.ClearBearerToken();

        // ── Step 2: Customers register, login and purchase ───────────────────
        AnsiConsole.MarkupLine("");

        int count = AnsiConsole.Ask<int>(
            $"[yellow]How many customers?[/] [grey](default: {settings.NumberOfCustomers})[/]",
            settings.NumberOfCustomers);

        AnsiConsole.MarkupLine($"\n[grey]Registering {count} customer(s) and purchasing through BFF...[/]\n");

        var enrolled = new List<(RegisterUserRequest Creds, LoginUserResponse Login)>();
        for (int i = 0; i < count; i++)
        {
            AnsiConsole.Write(new Rule($"[cyan]Customer {i + 1}/{count}[/]").RuleStyle("grey").LeftJustified());

            var customerReq = DataGenerator.FakeCustomer();
            AnsiConsole.MarkupLine($"[grey]→[/] Registering [cyan]{customerReq.Email}[/]...");
            var registered = await bff.RegisterAsync(customerReq, ct);
            AnsiConsole.MarkupLine($"  [green]✓[/] Registered: [bold]{registered!.Name}[/]");

            var login = await bff.LoginAsync(new LoginUserRequest(customerReq.Email, customerReq.Password), ct);
            AnsiConsole.MarkupLine("  [green]✓[/] Token obtained");

            bff.SetBearerToken(login!.AccessToken);
            var purchase = await bff.PurchaseAsync(new PurchaseCourseRequest(course.CourseId, course.Price), ct);
            AnsiConsole.MarkupLine($"  [green]✓[/] Purchased (PurchaseID: {purchase!.PurchaseId})");
            bff.ClearBearerToken();

            enrolled.Add((customerReq, login));
            AnsiConsole.MarkupLine("");
        }

        // ── Step 3: Random watch simulation ──────────────────────────────────
        var orderedLessons = lessons.OrderBy(l => l.Position).ToList();
        var profiles = AssignProfiles(enrolled, orderedLessons.Count);
        PrintProfileTable(profiles, orderedLessons.Count);

        AnsiConsole.MarkupLine($"\n[bold]Starting watch simulation via BFF…[/] [grey]Ctrl+C to stop.[/]\n");

        var watchTasks = profiles
            .Where(p => p.LessonsToWatch > 0)
            .Select(p => WatchLessonsAsync(settings, p, orderedLessons, ct));

        try
        {
            await Task.WhenAll(watchTasks);
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("\n[yellow]⚠ Simulation cancelled.[/]");
            return;
        }

        AnsiConsole.MarkupLine("");
        PrintResultTable(profiles, orderedLessons.Count);

        // ── Step 4: Read BFF aggregation endpoints ───────────────────────────
        AnsiConsole.MarkupLine("");
        AnsiConsole.Write(new Rule("[bold magenta]BFF Aggregation — CoursePage + Dashboard[/]").RuleStyle("magenta"));
        AnsiConsole.MarkupLine("");

        // CoursePage (unauthenticated view — anonymous customer browsing)
        var anonBff = new BffApiClient(settings.BaseUrl);
        var coursePage = await anonBff.GetCoursePageAsync(course.CourseId, ct);
        if (coursePage is not null)
        {
            AnsiConsole.MarkupLine($"[bold]CoursePage[/] (anonymous): [italic]{Markup.Escape(coursePage.Title)}[/]");
            AnsiConsole.MarkupLine($"  Lessons: {coursePage.Lessons.Count} | Enrolled: {coursePage.Enrollment.IsEnrolled}");
        }

        AnsiConsole.MarkupLine("");

        // Dashboard per customer (authenticated view)
        var dashboardTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Magenta1)
            .AddColumn("[magenta]Customer[/]")
            .AddColumn("[magenta]Enrolled Courses[/]")
            .AddColumn("[magenta]Completed Lessons[/]")
            .AddColumn("[magenta]Progress[/]");

        foreach (var (creds, login) in enrolled)
        {
            var customerBff = new BffApiClient(settings.BaseUrl);
            customerBff.SetBearerToken(login.AccessToken);

            DashboardResponse? dashboard = null;
            try { dashboard = await customerBff.GetDashboardAsync(ct); }
            catch { /* token may have expired in long simulations */ }

            if (dashboard is null || dashboard.EnrolledCourses.Count == 0)
            {
                dashboardTable.AddRow(creds.Email, "0", "-", "-");
                continue;
            }

            var item = dashboard.EnrolledCourses.FirstOrDefault(c => c.CourseId == course.CourseId);
            if (item is null)
            {
                dashboardTable.AddRow(creds.Email, dashboard.EnrolledCourses.Count.ToString(), "-", "-");
                continue;
            }

            var progressBar = item.Progress.Percentage >= 100
                ? "[bold green]100%[/]"
                : $"[yellow]{item.Progress.Percentage:F0}%[/]";

            dashboardTable.AddRow(
                Markup.Escape(creds.Email),
                dashboard.EnrolledCourses.Count.ToString(),
                $"{item.Progress.CompletedLessons}/{item.Progress.TotalLessons}",
                progressBar);
        }

        AnsiConsole.Write(dashboardTable);
    }

    // ── Profile assignment ───────────────────────────────────────────────────

    private static List<CustomerRun> AssignProfiles(
        List<(RegisterUserRequest Creds, LoginUserResponse Login)> enrolled, int totalLessons)
    {
        var rng = Random.Shared;
        var profiles = new List<CustomerRun>();

        for (int i = 0; i < enrolled.Count; i++)
        {
            var (creds, login) = enrolled[i];
            double roll = rng.NextDouble();
            int lessonsToWatch;
            bool finishesLast;

            if (roll < 0.15)
            {
                lessonsToWatch = 0;
                finishesLast = false;
            }
            else if (roll < 0.80)
            {
                lessonsToWatch = totalLessons <= 1 ? 1 : rng.Next(1, totalLessons);
                finishesLast = rng.NextDouble() < 0.40;
            }
            else
            {
                lessonsToWatch = totalLessons;
                finishesLast = true;
            }

            profiles.Add(new CustomerRun(i + 1, creds.Email, login.AccessToken, lessonsToWatch, finishesLast));
        }

        return profiles;
    }

    // ── Watch simulation (per customer, via BFF) ─────────────────────────────

    private static async Task WatchLessonsAsync(
        SimulatorSettings settings,
        CustomerRun profile,
        List<GetLessonResponse> lessons,
        CancellationToken ct)
    {
        var client = new BffApiClient(settings.BaseUrl);
        client.SetBearerToken(profile.Token);

        for (int i = 0; i < profile.LessonsToWatch; i++)
        {
            ct.ThrowIfCancellationRequested();

            var lesson = lessons[i];
            bool isLastWatched = i == profile.LessonsToWatch - 1;
            bool isCompleted = !isLastWatched || profile.FinishesLastLesson;
            int seconds = isCompleted
                ? DataGenerator.FakeWatchedSeconds(maxSeconds: 600)
                : DataGenerator.FakeWatchedSeconds(maxSeconds: 120);

            AnsiConsole.MarkupLine(
                $"  [magenta]Customer {profile.Index}[/] ([grey]{profile.Email}[/]) " +
                $"watching [italic]\"{Markup.Escape(lesson.Title)}\"[/] " +
                $"[grey]({seconds}s, {(isCompleted ? "completed" : "partial")})[/] [dim]via BFF[/]...");

            await Task.Delay(settings.WatchDelayMs, ct);
            await client.UpdateProgressAsync(new UpdateProgressRequest(lesson.LessonId, seconds, isCompleted), ct);
        }

        var outcome = profile.LessonsToWatch == lessons.Count
            ? "[bold green]finished[/]"
            : $"[yellow]dropped at lesson {profile.LessonsToWatch}/{lessons.Count}[/]";
        AnsiConsole.MarkupLine($"  [magenta]Customer {profile.Index}[/] → {outcome}");
    }

    // ── Console output ───────────────────────────────────────────────────────

    private static void PrintProfileTable(List<CustomerRun> profiles, int totalLessons)
    {
        var table = new Table().Border(TableBorder.Simple).BorderColor(Color.Magenta1);
        table.AddColumn("[magenta]#[/]");
        table.AddColumn("[magenta]Email[/]");
        table.AddColumn("[magenta]Assigned progression[/]");

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
        int finished     = profiles.Count(p => p.LessonsToWatch == totalLessons);
        int partial      = profiles.Count - neverStarted - finished;

        var summary = new Table().Border(TableBorder.Rounded).BorderColor(Color.Magenta1);
        summary.AddColumn("[magenta]Outcome[/]");
        summary.AddColumn("[magenta]Count[/]");
        summary.AddRow("[grey]Never started[/]",       neverStarted.ToString());
        summary.AddRow("[yellow]Partial (dropped)[/]", partial.ToString());
        summary.AddRow("[green]Finished course[/]",    finished.ToString());
        summary.AddRow("[bold]Total[/]",               profiles.Count.ToString());
        AnsiConsole.Write(summary);
    }
}
