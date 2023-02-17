using System.Diagnostics.CodeAnalysis;
using ChangeLog.Classes;
using ChangeLog.Liquibase;
using ChangeLog.Liquibase.ChangeTypes;
using ChangeLog.Utils;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChangeLog.Command;

public class UpdateCommand : AsyncCommand<UpdateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-s|--sourceConnectionString")]
        public string SourceConnectionString { get; set; } = "";

        [CommandOption("-f|--file")]
        public string File { get; set; } = "changeLog.yml";

        [CommandArgument(1, "[Output]")]
        public string Output { get; set; } = "./procs";
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var config = ConfigHelper.GetConfig(settings.File, null, settings.SourceConnectionString);
        Builder builder = new(config);
        if (!builder.Validate())
        {
            return await Task.FromResult(1);
        }

        var metaResults = await AnsiConsole
            .Status()
            .StartAsync("Getting target meta...", _ => Data.Db.GetMeta(builder, true));
        var meta = metaResults.Where(x => x.Type.Equals("P") && x.Definition.Length > 0).ToList();

        AnsiConsole.MarkupLine($"Found [green]{meta.Count}[/] stored procedures in database");
        ProcResults results = Procs.GetProcsFromFiles(settings.Output);
        AnsiConsole.MarkupLine($"Found [green]{results.Procs.Count}[/] stored procedures in procs folder");

        string dir = Path.GetFullPath(settings.Output);
        Directory.CreateDirectory(dir);

        int added = 0;
        int updated = 0;
        int deleted = 0;

        var metaDictionary = meta.ToDictionary(x => $"{x.Schema}.{x.Name}", x => x);

        foreach (var d in results.Procs)
        {
            if (!metaDictionary.ContainsKey(d.Key))
            {
                deleted++;
            }
        }

        foreach (var m in meta)
        {
            if (results.Procs.ContainsKey($"{m.Schema}.{m.Name}"))
            {
                updated++;
            }
            else
            {
                added++;
            }

            LiquibaseContainer changeLog = new()
            {
                DatabaseChangeLog = new()
                {
                    new DatabaseChangeLog {
                        ChangeSet = new () {
                            RunOnChange = true,
                            Author = Environment.UserName,
                            Id = m.Name,
                            Rollback = "empty",
                            Changes = new() {
                                new ChangeType
                                {
                                    CreateProcedure = new CreateProcedureType {
                                        SchemaName = m.Schema,
                                        ProcedureName = m.Name,
                                        ProcedureBody = m.Definition,
                                        ReplaceIfExists = true
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // string path = Path.GetFullPath(Path.Combine(dir, $"{m.Schema}.{m.Name}.yml"));
            // var yaml = Yaml.GetSerializer().Serialize(changeLog);
            // File.WriteAllText(path, yaml);
        }

        AnsiConsole.MarkupLine($"[yellow] Updating: {updated}[/]");
        AnsiConsole.MarkupLine($"[green] Added: {added}[/]");
        AnsiConsole.MarkupLine($"[red] Deleted: {deleted}[/]");

        return await Task.FromResult(0);
    }
}
