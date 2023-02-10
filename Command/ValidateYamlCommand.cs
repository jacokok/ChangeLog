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

        // BuilderVerify verify = new(settings.File);
        // var builder = verify.ValidateYaml();
        // if (BuilderVerify.ValidateBuilder(builder))
        // {
        //     return BuilderVerify.HasValidDbConnection(builder) ? 0 : 1;
        // }
        return await Task.FromResult(1);
    }
}
