using System.Diagnostics.CodeAnalysis;
using ChangeLog.Classes;
using ChangeLog.Data;
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

        [CommandOption("-d|--dry-run")]
        public bool DryRun { get; set; }
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
            .StartAsync("Getting target meta...", _ => Db.GetMeta(builder, true));
        var meta = metaResults.Where(x => x.Type.Equals("P") && x.Definition.Length > 0).ToList();

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[purple]Dry run: will not make any changes to files[/]");
        }

        AnsiConsole.MarkupLine($"Found [green]{meta.Count}[/] stored procedures in database");
        ProcResults results = Procs.GetProcsFromFiles(settings.Output);
        AnsiConsole.MarkupLine($"Found [green]{results.Procs.Count}[/] stored procedures in procs folder");

        string dir = Path.GetFullPath(settings.Output);
        Directory.CreateDirectory(dir);

        int added = 0;
        int updated = 0;
        int matched = 0;
        int deleted = 0;

        var metaDictionary = meta.ToDictionary(x => $"{x.Schema}.{x.Name}", x => x);

        foreach (var d in results.Procs)
        {
            if (!metaDictionary.ContainsKey(d.Key))
            {
                deleted++;
                if (!settings.DryRun)
                {
                    var dbChangeLog = d.Value.DatabaseChangeLog?.FirstOrDefault();
                    var changes = dbChangeLog?.ChangeSet?.Changes?.FirstOrDefault();
                    string? pName = changes?.CreateProcedure?.ProcedureName;
                    string? pSchema = changes?.CreateProcedure?.SchemaName;
                    string pText = changes?.CreateProcedure?.ProcedureText ?? string.Empty;

                    if (pName != null)
                    {
                        LiquibaseContainer deleteChangeLog = new()
                        {
                            DatabaseChangeLog = new()
                            {
                                new DatabaseChangeLog {
                                    ChangeSet = new () {
                                        Author = Environment.UserName,
                                        Id = $"Drop{pSchema}{pName}",
                                        Changes = new() {
                                            new ChangeType
                                            {
                                                CreateProcedure = new CreateProcedureType {
                                                    SchemaName = pSchema,
                                                    ReplaceIfExists = true,
                                                    ProcedureName = pName,
                                                    ProcedureText = pText
                                                },
                                                DropProcedure = new DropProcedureType {
                                                    SchemaName = pSchema,
                                                    ProcedureName = pName
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        };
                        string deletedDir = Path.GetFullPath(Path.Combine(dir, "deleted"));
                        Directory.CreateDirectory(deletedDir);
                        string deletedPath = Path.GetFullPath(Path.Combine(deletedDir, $"{pSchema}.{pName}.yml"));
                        var yaml = Yaml.GetSerializer().Serialize(deleteChangeLog);
                        File.WriteAllText(deletedPath, yaml);

                        string oldFilePath = Path.GetFullPath(Path.Combine(dir, $"{pSchema}.{pName}.yml"));
                        File.Delete(oldFilePath);
                    }
                }
            }
        }

        foreach (var m in meta)
        {
            bool isMatch = false;
            string fileDefinition;
            if (results.Procs.ContainsKey($"{m.Schema}.{m.Name}"))
            {
                var fileDbChangeLog = results.Procs.FirstOrDefault(x => x.Key.Equals($"{m.Schema}.{m.Name}", StringComparison.Ordinal));
                var dbChangeLog = fileDbChangeLog.Value.DatabaseChangeLog?.FirstOrDefault();
                var change = dbChangeLog?.ChangeSet?.Changes?.FirstOrDefault();
                fileDefinition = change?.CreateProcedure?.ProcedureText ?? "";
                isMatch = Generator.DefinitionCleanup(Generator.DefinitionCustomCleanup(fileDefinition, m.Schema, m.Name)) == Generator.DefinitionCleanup(Generator.DefinitionCustomCleanup(m.Definition, m.Schema, m.Name));
                if (isMatch)
                {
                    matched++;
                }
                else
                {
                    updated++;
                }
            }
            else
            {
                added++;
            }

            if (!isMatch && !settings.DryRun)
            {
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
                                            ProcedureText = m.Definition,
                                            ReplaceIfExists = true
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
                string path = Path.GetFullPath(Path.Combine(dir, $"{m.Schema}.{m.Name}.yml"));
                var yaml = Yaml.GetSerializer().Serialize(changeLog);
                File.WriteAllText(path, yaml);
            }
        }

        AnsiConsole.MarkupLine($"[blue] Matched: {matched}[/]");
        AnsiConsole.MarkupLine($"[yellow] Updating: {updated}[/]");
        AnsiConsole.MarkupLine($"[green] Added: {added}[/]");
        AnsiConsole.MarkupLine($"[red] Deleted: {deleted}[/]");

        return await Task.FromResult(0);
    }
}
