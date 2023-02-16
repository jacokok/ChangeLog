using System.Diagnostics.CodeAnalysis;
using ChangeLog.Classes;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChangeLog.Command;

public class ValidateYamlCommand : AsyncCommand<ValidateYamlCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[File]")]
        public string File { get; set; } = "changeLog.yml";
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        AnsiConsole.MarkupLine($"Validating file: [blue]{settings.File}[/]");
        Builder builder = new(settings.File);
        builder.ValidateAll();

        return await Task.FromResult(0);
    }
}
