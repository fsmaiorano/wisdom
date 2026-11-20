using Spectre.Console;
using Wisdom.Simulator.Config;
using Wisdom.Simulator.Scenarios;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

AnsiConsole.Clear();
AnsiConsole.Write(
    new FigletText("Wisdom")
        .Centered()
        .Color(Color.Gold3_1));

AnsiConsole.Write(new Rule("[grey]System Simulator[/]").RuleStyle("grey"));
AnsiConsole.MarkupLine("");

var settings = new SimulatorSettings();

var envOverride = Environment.GetEnvironmentVariable("BASE_URL");
if (envOverride is not null)
{
    settings.BaseUrl = envOverride;
}
else
{
    var env = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold]Select environment:[/]")
            .AddChoices(["Local (http://localhost:5000)", "Docker (http://localhost:6100)"]));

    settings.BaseUrl = env.StartsWith("Docker") ? "http://localhost:6100" : "http://localhost:5000";
}

PrintApiUrls(settings);
AnsiConsole.MarkupLine("");

while (!cts.Token.IsCancellationRequested)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold]Choose a scenario to run:[/]")
            .AddChoices([
                "1 — Create course with lessons",
                "2 — Create course, lessons and buy",
                "3 — Create course, lessons, X users buy and watch (loop)",
                "4 — Create course, X users buy and watch with random progression",
                "5 — Create course, X users buy, watch and shows BFF aggregation",
                "Exit"
            ]));

    if (choice.StartsWith("Exit"))
        break;

    var ctx = new SimulatorContext(settings);

    try
    {
        AnsiConsole.MarkupLine("");
        if (choice.StartsWith("1"))
            await Scenario1.RunAsync(ctx, cts.Token);
        else if (choice.StartsWith("2"))
            await Scenario2.RunAsync(ctx, cts.Token);
        else if (choice.StartsWith("3"))
            await Scenario3.RunAsync(ctx, cts.Token);
        else if (choice.StartsWith("4"))
            await Scenario4.RunAsync(ctx, cts.Token);
        else if (choice.StartsWith("5"))
            await Scenario5.RunAsync(settings, cts.Token);
    }
    catch (OperationCanceledException)
    {
        AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
        break;
    }
    catch (HttpRequestException ex)
    {
        AnsiConsole.MarkupLine($"[red]HTTP error:[/] {Markup.Escape(ex.Message)}");
        AnsiConsole.MarkupLine("[grey]Make sure the BFF is running: dotnet run --project Wisdom.Bff[/]");
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
    }

    AnsiConsole.MarkupLine("");
    AnsiConsole.Write(new Rule("[grey]Done[/]").RuleStyle("grey"));
    AnsiConsole.MarkupLine("");
}

AnsiConsole.MarkupLine("[grey]Goodbye![/]");

static void PrintApiUrls(SimulatorSettings s)
{
    var table = new Table().Border(TableBorder.Simple).BorderColor(Color.Grey);
    table.AddColumn("[grey]Entry point[/]");
    table.AddColumn("[grey]URL[/]");
    table.AddColumn("[grey]Override[/]");
    table.AddRow("BFF (all traffic)", $"[magenta]{s.BaseUrl}[/]", "[grey]BASE_URL[/]");
    AnsiConsole.Write(table);
}
