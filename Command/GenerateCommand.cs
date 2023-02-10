using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using ChangeLog.Classes;
using ChangeLog.Liquibase;
using ChangeLog.Liquibase.ChangeTypes;
using ChangeLog.Utils;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChangeLog.Command;

public class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-s|--sourceConnectionString")]
        public string SourceConnectionString { get; set; } = "";

        [CommandOption("-f|--file")]
        public string File { get; set; } = "changeLog.yml";

        [Description(Description.GenerateTypeDescription)]
        [CommandOption("-t|--type")]
        public string? Type { get; set; }
        [CommandArgument(1, "[OutputFile]")]
        public string Output { get; set; } = "./output.yml";
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var config = ConfigHelper.GetConfig(settings.File, null, settings.SourceConnectionString);
        Builder builder = new(config);
        if (!builder.Validate())
        {
            return await Task.FromResult(0);
        }

        var metaResults = await AnsiConsole
            .Status()
            .StartAsync("Getting target meta...", _ => Data.Db.GetMeta(builder, true));

        metaResults = (settings.Type != null) ? metaResults.Where(x => x.Type.Equals(settings.Type)).ToList() : metaResults;

        AnsiConsole.MarkupLine($"Generating count: [green]{metaResults.Count}[/]");

        string path = Path.GetFullPath(settings.Output);
        LiquibaseContainer changeLog = new()
        {
            DatabaseChangeLog = new()
        };

        foreach (var m in metaResults)
        {
            if (m.Type == "P")
            {
                changeLog.DatabaseChangeLog.Add(
                    new DatabaseChangeLog
                    {
                        ChangeSet = new()
                        {
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
                                        ProcedureBody = m.Definition.Trim(),
                                        ReplaceIfExists = true
                                    }
                                }
                            }
                        }
                    }
                );
            }
            else if (m.Type == "FN" || m.Type == "IF" || m.Type == "TF")
            {
                changeLog.DatabaseChangeLog.Add(
                    new DatabaseChangeLog
                    {
                        ChangeSet = new()
                        {
                            Author = Environment.UserName,
                            Id = m.Name,
                            Rollback = $"DROP FUNCTION {m.Schema}.{m.Name}",
                            Changes = new() {
                                new ChangeType
                                {
                                    Sql = new SqlType {
                                        Sql = m.Definition.Trim()
                                    }
                                }
                            }
                        }
                    }
                );
            }
        }
        var yaml = Yaml.GetSerializer().Serialize(changeLog);
        File.WriteAllText(path, yaml);

        return await Task.FromResult(1);
    }
}
