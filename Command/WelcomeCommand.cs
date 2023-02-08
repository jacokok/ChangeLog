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
            new FigletText("Change Log")
            .Centered()
            .Color(turquoise));

        if (settings.Version)
        {
            AnsiConsole.Write(
                new FigletText(GetVersion())
                .Centered()
                .Color(turquoise));

            AnsiConsole.MarkupLine("version: " + GetVersion());
        }
        else
        {
            // Use "dotnet ef [command] --help" for more information about a command.
            AnsiConsole.MarkupLine("ChangeLog " + GetVersion());
            AnsiConsole.MarkupLine("Use [yellow]changelog -h[/] for more information");
        }

        return 0;
    }
}
