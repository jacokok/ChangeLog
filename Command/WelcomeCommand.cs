using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChangeLog.Command;

internal sealed class WelcomeCommand : Command<WelcomeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-v|--version")]
        public bool Version { get; init; }
    }

    private static readonly Color turquoise = new(92, 190, 188);

    public static string GetVersion()
    {
        return typeof(WelcomeCommand)?.Assembly?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "?";
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        AnsiConsole.Write(
            new FigletText("changelog")
            .LeftJustified()
            .Color(turquoise));

        AnsiConsole.WriteLine();

        if (settings.Version)
        {
            AnsiConsole.MarkupLine("version: " + GetVersion());
        }
        else
        {
            AnsiConsole.MarkupLine("Use [yellow]changelog -h[/] for more information");
        }

        return 0;
    }
}
