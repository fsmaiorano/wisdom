using Spectre.Console;
using Wisdom.Simulator.Clients;
using Wisdom.Simulator.Config;
using Wisdom.Simulator.Helpers;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Scenarios;

/// <summary>
/// Scenario 6: Demo setup. The seeded teacher creates and publishes a course with varied lessons
/// (each with a video from mocks/) and the seeded student buys it. Nothing is marked as watched,
/// so signing in as the student shows a fully enrolled course ready to be played.
/// </summary>
public static class Scenario6
{
    public static async Task RunAsync(SimulatorSettings settings, CancellationToken ct)
    {
        var bff = new BffApiClient(settings.BaseUrl);

        AnsiConsole.Write(new Rule("[bold green]Scenario 6 — Demo Setup (seeded accounts)[/]").RuleStyle("green"));
        AnsiConsole.MarkupLine($"\n[grey]All calls routed through the BFF at [bold]{settings.BaseUrl}[/][/]\n");

        // ── Teacher: course + lessons ────────────────────────────────────────
        AnsiConsole.MarkupLine($"[grey]→[/] Logging in as seeded teacher [cyan]{settings.SeedTeacherEmail}[/]...");
        var teacherLogin = await bff.LoginAsync(new LoginUserRequest(settings.SeedTeacherEmail, settings.SeedTeacherPassword), ct);
        bff.SetBearerToken(teacherLogin!.AccessToken);
        AnsiConsole.MarkupLine("  [green]✓[/] Teacher token obtained");

        var courseReq = DataGenerator.DemoCourse();
        AnsiConsole.MarkupLine($"[grey]→[/] Creating course [italic]\"{Markup.Escape(courseReq.Title)}\"[/] @ ${courseReq.Price}...");
        var course = await bff.CreateCourseAsync(courseReq, ct);
        AnsiConsole.MarkupLine($"  [green]✓[/] Course created (ID: {course!.CourseId})");

        var mockFiles = DataGenerator.FindMockVideos();
        if (mockFiles.Length > 0)
            AnsiConsole.MarkupLine($"[grey]→[/] Found [bold]{mockFiles.Length}[/] mock video(s) — will cycle through them per lesson");
        else
            AnsiConsole.MarkupLine("[yellow]⚠[/]  No videos found in mocks/ — lessons will be created without video");

        var lessonRequests = DataGenerator.DemoLessons();
        var lessons = new List<GetLessonResponse>();
        AnsiConsole.MarkupLine($"[grey]→[/] Adding {lessonRequests.Length} lessons (uploading videos)...");
        for (int i = 0; i < lessonRequests.Length; i++)
        {
            var videoPath = mockFiles.Length > 0 ? mockFiles[i % mockFiles.Length] : null;
            var (lesson, uploadError) = await bff.CreateLessonAsync(
                course.CourseId, lessonRequests[i], videoPath, DataGenerator.FakeThumbnailUrl(), ct);
            lessons.Add(lesson!);

            var previewTag = lesson!.IsPreview ? " [grey](preview)[/]" : "";
            var uploadTag = uploadError is not null
                ? $" [yellow]⚠ upload: {Markup.Escape(uploadError)}[/]"
                : videoPath is null ? " [grey](no video)[/]" : " [blue]▶ video uploaded[/]";
            AnsiConsole.MarkupLine(
                $"    [green]+[/] [[{i + 1}/{lessonRequests.Length}]] [italic]{Markup.Escape(lesson.Title)}[/]{previewTag}{uploadTag}");
        }

        AnsiConsole.MarkupLine("[grey]→[/] Publishing course...");
        await bff.PublishCourseAsync(course.CourseId, true, ct);
        AnsiConsole.MarkupLine("  [green]✓[/] Course published");
        bff.ClearBearerToken();

        // ── Student: purchase ────────────────────────────────────────────────
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine($"[grey]→[/] Logging in as seeded student [cyan]{settings.SeedStudentEmail}[/]...");
        var studentLogin = await bff.LoginAsync(new LoginUserRequest(settings.SeedStudentEmail, settings.SeedStudentPassword), ct);
        bff.SetBearerToken(studentLogin!.AccessToken);
        AnsiConsole.MarkupLine("  [green]✓[/] Student token obtained");

        var purchase = await bff.PurchaseAsync(new PurchaseCourseRequest(course.CourseId, course.Price), ct);
        AnsiConsole.MarkupLine($"  [green]✓[/] Course purchased (PurchaseID: {purchase!.PurchaseId})");

        var page = await bff.GetCoursePageAsync(course.CourseId, ct);
        if (page is not null)
            AnsiConsole.MarkupLine(
                $"  [green]✓[/] Enrollment confirmed: [bold]{page.Enrollment.IsEnrolled}[/] — {page.Lessons.Count} lessons, " +
                $"{page.Progress.CompletedLessons}/{page.Progress.TotalLessons} completed");

        // ── Media processing ─────────────────────────────────────────────────
        if (mockFiles.Length > 0 && AnsiConsole.Confirm(
                "[yellow]Wait for the Media Worker to finish transcoding?[/] [grey](lessons are playable only when Ready)[/]", true))
        {
            await WaitForMediaAsync(bff, lessons, settings.MediaProcessingTimeoutSeconds, ct);
        }

        PrintSummary(settings, course, lessons);
    }

    private static async Task WaitForMediaAsync(
        BffApiClient bff, List<GetLessonResponse> lessons, int timeoutSeconds, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        var statuses = lessons.ToDictionary(l => l.LessonId, _ => "Pending");

        await AnsiConsole.Status().StartAsync("Waiting for media processing...", async status =>
        {
            while (!ct.IsCancellationRequested && DateTime.UtcNow < deadline)
            {
                foreach (var lesson in lessons)
                {
                    try
                    {
                        var media = await bff.GetLessonMediaAsync(lesson.LessonId, ct);
                        statuses[lesson.LessonId] = media?.FirstOrDefault(m => m.Type == "Video")?.Status ?? "Pending";
                    }
                    catch (HttpRequestException)
                    {
                        // media service may still be catching up; retry on next round
                    }
                }

                var ready = statuses.Values.Count(s => s == "Ready");
                var failed = statuses.Values.Count(s => s == "Failed");
                status.Status($"Waiting for media processing... {ready}/{lessons.Count} ready" +
                              (failed > 0 ? $", {failed} failed (the reconciler will retry)" : ""));

                if (statuses.Values.All(s => s == "Ready"))
                    return;

                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        });

        var table = new Table().Border(TableBorder.Simple).BorderColor(Color.Green);
        table.AddColumn("[green]Lesson[/]");
        table.AddColumn("[green]Media status[/]");
        foreach (var lesson in lessons.OrderBy(l => l.Position))
        {
            var s = statuses[lesson.LessonId];
            var color = s == "Ready" ? "green" : s == "Failed" ? "red" : "yellow";
            table.AddRow(Markup.Escape(lesson.Title), $"[{color}]{s}[/]");
        }
        AnsiConsole.Write(table);

        if (statuses.Values.Any(s => s != "Ready"))
            AnsiConsole.MarkupLine("[yellow]⚠[/]  Some media is not Ready yet. The worker keeps processing in the background — lessons become playable as they finish.");
    }

    private static void PrintSummary(SimulatorSettings settings, CreateCourseResponse course, List<GetLessonResponse> lessons)
    {
        var uiUrl = settings.BaseUrl.Contains("6100") ? "http://localhost:6001" : "http://localhost:4200";

        AnsiConsole.MarkupLine("");
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Green);
        table.AddColumn("[green]Field[/]");
        table.AddColumn("[green]Value[/]");
        table.AddRow("Course", Markup.Escape(course.Title));
        table.AddRow("Course ID", course.CourseId.ToString());
        table.AddRow("Lessons", lessons.Count.ToString());
        table.AddRow("Teacher", $"{settings.SeedTeacherEmail} / {settings.SeedTeacherPassword}");
        table.AddRow("Student (enrolled)", $"{settings.SeedStudentEmail} / {settings.SeedStudentPassword}");
        table.AddRow("UI", uiUrl);
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[green]✓[/] Sign in as the student to open the course from the dashboard.");
    }
}
