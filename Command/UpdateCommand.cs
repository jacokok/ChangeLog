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

        [CommandOption("-i|--interactive")]
        public bool Interactive { get; set; }
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
        if (settings.Interactive)
        {
            AnsiConsole.MarkupLine("[purple]Interactive: running in interactive mode[/]");
        }

        AnsiConsole.MarkupLine($"Found [green]{meta.Count}[/] stored procedures in database");
        ProcResults results = Procs.GetProcsFromFiles(settings.Output);
        AnsiConsole.MarkupLine($"Found [green]{results.Procs.Count}[/] stored procedures in procs folder");

        string dir = Path.GetFullPath(settings.Output);
        if (!settings.DryRun)
        {
            Directory.CreateDirectory(dir);
        }

        int matched = 0;
        List<UpdatedMetaDTO> resultList = new();

        var metaDictionary = meta.ToDictionary(x => $"{x.Schema}.{x.Name}", x => x);

        foreach (var d in results.Procs)
        {
            if (!metaDictionary.ContainsKey(d.Key))
            {
                resultList.Add(new UpdatedMetaDTO
                {
                    Key = d.Key,
                    ActionType = ActionType.Delete,
                    Proc = d.Value
                });
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
                    resultList.Add(new UpdatedMetaDTO
                    {
                        Key = $"{m.Schema}.{m.Name}",
                        ActionType = ActionType.Match,
                        Meta = m
                    });
                }
                else
                {
                    resultList.Add(new UpdatedMetaDTO
                    {
                        Key = $"{m.Schema}.{m.Name}",
                        ActionType = ActionType.Update,
                        Meta = m
                    });
                }
            }
            else
            {
                resultList.Add(new UpdatedMetaDTO
                {
                    Key = $"{m.Schema}.{m.Name}",
                    ActionType = ActionType.Add,
                    Meta = m
                });
            }
        }

        if (settings.Interactive)
        {
            if (resultList.Any(x => x.ActionType != ActionType.Match))
            {
                var pickedItems = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("Pick [green]items[/]?")
                        .NotRequired()
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to reveal more items)[/]")
                        .InstructionsText(
                            "[grey](Press [blue]<space>[/] to toggle item, " +
                            "[green]<enter>[/] to accept)[/]")
                        .AddChoiceGroup("[green]Add[/]", resultList.Where(x => x.ActionType.Equals(ActionType.Add))?.Select(m => m.Key).ToArray() ?? Array.Empty<string>())
                        .AddChoiceGroup("[yellow]Update[/]", resultList.Where(x => x.ActionType.Equals(ActionType.Update))?.Select(m => m.Key).ToArray() ?? Array.Empty<string>())
                        .AddChoiceGroup("[red]Delete[/]", resultList.Where(x => x.ActionType.Equals(ActionType.Delete))?.Select(m => m.Key).ToArray() ?? Array.Empty<string>())
                    );
                resultList = FilterPickedItems(pickedItems, resultList);
            }
        }
        var counts = GetCounts(resultList);

        if (!settings.DryRun)
        {
            UpdateItems(resultList, dir);
            AnsiConsole.MarkupLine($"[blue] Matched: {matched}[/]");
            AnsiConsole.MarkupLine($"[yellow] Updated: {counts.Update}[/]");
            AnsiConsole.MarkupLine($"[green] Added: {counts.Add}[/]");
            AnsiConsole.MarkupLine($"[red] Deleted: {counts.Delete}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[blue] Matched: {matched}[/]");
            AnsiConsole.MarkupLine($"[yellow] To update: {counts.Update}[/]");
            AnsiConsole.MarkupLine($"[green] To add: {counts.Add}[/]");
            AnsiConsole.MarkupLine($"[red] To delete: {counts.Delete}[/]");
        }

        return await Task.FromResult(0);
    }

    private static List<UpdatedMetaDTO> FilterPickedItems(List<string> pickedItems, List<UpdatedMetaDTO> resultList)
    {
        foreach (var item in pickedItems)
        {
            AnsiConsole.MarkupLine($"[red] Item: {item}[/]");
        }
        foreach (var item in resultList.Where(x => x.ActionType == ActionType.Add).ToList())
        {
            AnsiConsole.MarkupLine($"[red] Item: {item.Key}[/]");
        }
        return resultList.Where(x => pickedItems.Contains(x.Key)).ToList();
    }

    private static Counts GetCounts(List<UpdatedMetaDTO> resultList)
    {
        var add = resultList.Count(x => x.ActionType == ActionType.Add);
        var update = resultList.Count(x => x.ActionType == ActionType.Update);
        var delete = resultList.Count(x => x.ActionType == ActionType.Delete);
        return new Counts { Add = add, Update = update, Delete = delete };
    }

    private static void UpdateItems(List<UpdatedMetaDTO> resultList, string dir)
    {
        foreach (var item in resultList)
        {
            if (item.ActionType == ActionType.Delete)
            {
                DeleteProc(item.Proc, dir);
            }
            else if (item.ActionType == ActionType.Update || item.ActionType == ActionType.Add)
            {
                if (item.Meta is not null)
                {
                    LiquibaseContainer changeLog = new()
                    {
                        DatabaseChangeLog = new()
                        {
                            new DatabaseChangeLog {
                                ChangeSet = new () {
                                    RunOnChange = true,
                                    Author = Environment.UserName,
                                    Id =item.Meta.Name,
                                    Rollback = "empty",
                                    Changes = new() {
                                        new ChangeType
                                        {
                                            CreateProcedure = new CreateProcedureType {
                                                SchemaName = item.Meta.Schema,
                                                ProcedureName = item.Meta.Name,
                                                ProcedureText = item.Meta.Definition,
                                                ReplaceIfExists = true
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };
                    string path = Path.GetFullPath(Path.Combine(dir, $"{item.Meta?.Schema}.{item.Meta?.Name}.yml"));
                    var yaml = Yaml.GetSerializer().Serialize(changeLog);
                    File.WriteAllText(path, yaml);
                }
            }
        }
    }

    private static void DeleteProc(LiquibaseContainer? container, string dir)
    {
        if (container is not null)
        {
            var dbChangeLog = container.DatabaseChangeLog?.FirstOrDefault();
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
                                                DropProcedure = new DropProcedureType {
                                                    SchemaName = pSchema,
                                                    ProcedureName = pName
                                                }
                                            }
                                        },
                                        Rollback = pText,
                                        PreConditions = new List<PreCondition> () {
                                            new PreCondition () {
                                                OnFail = "MARK_RAN",
                                                SqlCheck = new () {
                                                    ExpectedResult = "1",
                                                    Sql = $"SELECT COUNT(OBJECT_ID('{pSchema}.{pName}', 'P'))"
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
