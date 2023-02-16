using System.Diagnostics.CodeAnalysis;
using System.Text;
using ChangeLog.Classes;
using ChangeLog.Liquibase;
using ChangeLog.Liquibase.ChangeTypes;
using ChangeLog.Utils;
using Spectre.Console;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ChangeLog.Command;

public class InitCommand : AsyncCommand<InitCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        if (File.Exists("./changeLog.yml"))
        {
            AnsiConsole.MarkupLine("[yellow]changeLog.yml[/] already exists");
            return await Task.FromResult(1);
        }
        Config config = new()
        {
            ConnectionString = "Data Source=localhost;Initial Catalog=Test;UID=sa;PWD=Admin12345;Max Pool Size=1;Min Pool Size=1;Pooling=True;TrustServerCertificate=True;",
            SourceConnectionString = "Data Source=localhost;Initial Catalog=Test2;UID=sa;PWD=Admin12345;Max Pool Size=1;Min Pool Size=1;Pooling=True;TrustServerCertificate=True;"
        };

        var yaml = Yaml.GetSerializer().Serialize(config);
        File.WriteAllText("./changeLog.yml", yaml);

        AnsiConsole.Markup("[bold green]:party_popper: Created changeLog.yml.[/] ");
        return await Task.FromResult(0);
    }
}
