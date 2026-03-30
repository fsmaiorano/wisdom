using Spectre.Console;
using Wisdom.Simulator.Helpers;
using Wisdom.Simulator.Models;

namespace Wisdom.Simulator.Scenarios;

/// <summary>
/// Scenario 1: A teacher registers, logs in, creates a course with N lessons (each with a video), and publishes it.
/// </summary>
public static class Scenario1
{
    public static async Task RunAsync(SimulatorContext ctx, CancellationToken ct)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Scenario 1 — Course Creation[/]").RuleStyle("yellow"));
        AnsiConsole.MarkupLine("");

        // ── Register teacher ────────────────────────────────────────────────
        var teacherReq = DataGenerator.FakeTeacher();
        ctx.TeacherCredentials = teacherReq;

        AnsiConsole.MarkupLine($"[grey]→[/] Registering teacher [cyan]{teacherReq.Email}[/]...");
        var registered = await ctx.UserClient.RegisterAsync(teacherReq, ct);
        AnsiConsole.MarkupLine($"  [green]✓[/] Teacher registered: [bold]{registered!.Name}[/] (ID: {registered.UserId})");

        // ── Login ────────────────────────────────────────────────────────────
        AnsiConsole.MarkupLine($"[grey]→[/] Logging in...");
        var loginResp = await ctx.UserClient.LoginAsync(new LoginUserRequest(teacherReq.Email, teacherReq.Password), ct);
        ctx.TeacherToken = loginResp;
        ctx.UserClient.SetBearerToken(loginResp!.AccessToken);
        ctx.CourseClient.SetBearerToken(loginResp.AccessToken);
        AnsiConsole.MarkupLine($"  [green]✓[/] Token obtained (expires in {loginResp.ExpiresIn}s)");

        // ── Create course ────────────────────────────────────────────────────
        var courseReq = DataGenerator.FakeCourse();
        AnsiConsole.MarkupLine($"[grey]→[/] Creating course [italic]\"{courseReq.Title}\"[/] @ ${courseReq.Price}...");
        var course = await ctx.CourseClient.CreateCourseAsync(courseReq, ct);
        ctx.Course = course;
        AnsiConsole.MarkupLine($"  [green]✓[/] Course created (ID: {course!.CourseId})");

        // ── Discover mock videos ─────────────────────────────────────────────
        var mockFiles = FindMockVideos();
        if (mockFiles.Length > 0)
            AnsiConsole.MarkupLine($"[grey]→[/] Found [bold]{mockFiles.Length}[/] mock video(s) — will cycle through them per lesson");
        else
            AnsiConsole.MarkupLine("[yellow]⚠[/]  No .mkv files found in mocks/ — lessons will be created without video");

        // ── Add lessons ──────────────────────────────────────────────────────
        AnsiConsole.MarkupLine($"[grey]→[/] Adding {ctx.Settings.LessonsPerCourse} lessons...");
        for (int i = 1; i <= ctx.Settings.LessonsPerCourse; i++)
        {
            var lessonReq = DataGenerator.FakeLesson(i);
            var videoPath = mockFiles.Length > 0 ? mockFiles[(i - 1) % mockFiles.Length] : null;
            var thumbnailUrl = DataGenerator.FakeThumbnailUrl();

            var lesson = await ctx.CourseClient.CreateLessonAsync(course.CourseId, lessonReq, videoPath, thumbnailUrl, ct);
            ctx.Lessons.Add(lesson!);

            var previewTag = lesson!.IsPreview ? " [grey](preview)[/]" : "";
            var mediaTag = lesson.MediaId.HasValue
                ? $" [blue]▶ media:{lesson.MediaId}[/]"
                : " [grey](no video)[/]";
            AnsiConsole.MarkupLine($"    [green]+[/] [[{i}/{ctx.Settings.LessonsPerCourse}]] [italic]{Markup.Escape(lesson.Title)}[/]{previewTag}{mediaTag}");
        }

        // ── Publish ──────────────────────────────────────────────────────────
        AnsiConsole.MarkupLine($"[grey]→[/] Publishing course...");
        var published = await ctx.CourseClient.PublishCourseAsync(course.CourseId, true, ct);
        AnsiConsole.MarkupLine($"  [green]✓[/] Course is now [bold green]published[/] (IsPublished: {published!.IsPublished})");

        AnsiConsole.MarkupLine("");
        PrintSummaryTable(course, ctx.Lessons);
    }

    /// <summary>Walks up from the binary directory until a sibling "mocks/" folder is found.</summary>
    private static string[] FindMockVideos()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var mocks = Path.Combine(dir.FullName, "mocks");
            if (Directory.Exists(mocks))
                return Directory.GetFiles(mocks, "*.mkv", SearchOption.TopDirectoryOnly)
                    .Order()
                    .ToArray();
            dir = dir.Parent;
        }
        return [];
    }

    private static void PrintSummaryTable(Models.CreateCourseResponse course, List<GetLessonResponse> lessons)
    {
        var withVideo = lessons.Count(l => l.MediaId.HasValue);

        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Yellow);
        table.AddColumn("[yellow]Field[/]");
        table.AddColumn("[yellow]Value[/]");
        table.AddRow("Course ID", course.CourseId.ToString());
        table.AddRow("Title", Markup.Escape(course.Title));
        table.AddRow("Price", $"${course.Price}");
        table.AddRow("Lessons", lessons.Count.ToString());
        table.AddRow("With video", $"{withVideo}/{lessons.Count}");
        table.AddRow("Published", "[green]Yes[/]");
        AnsiConsole.Write(table);
    }
}
