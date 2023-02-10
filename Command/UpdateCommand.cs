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
        [CommandArgument(0, "[File]")]
        public string File { get; set; } = "changeLog.yml";
        [CommandArgument(1, "[Output]")]
        public string Output { get; set; } = "./output";
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        Builder builder = new(settings.File);
        var metaResults = await Data.Db.GetMeta(builder, false);
        var meta = metaResults.Where(x => x.Type.Equals("P") && x.Definition.Length > 0).ToList();

        AnsiConsole.MarkupLine($"Generating count: [green]{meta.Count}[/]");

        string dir = Path.GetFullPath(settings.Output);
        Directory.CreateDirectory(dir);

        foreach (var m in meta)
        {
            string path = Path.GetFullPath(Path.Combine(dir, $"{m.Name}.yaml"));

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
            var yaml = Yaml.GetSerializer().Serialize(changeLog);
            File.WriteAllText(path, yaml);
        }

        return await Task.FromResult(1);
    }
}
