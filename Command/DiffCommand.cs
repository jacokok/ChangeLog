using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using ChangeLog.Classes;
using ChangeLog.Data;
using ChangeLog.Utils;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChangeLog.Command;

public class DiffCommand : AsyncCommand<DiffCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-c|--connectionString")]
        public string ConnectionString { get; set; } = "";
        [CommandOption("-s|--sourceConnectionString")]
        public string SourceConnectionString { get; set; } = "";

        [CommandOption("-f|--file")]
        public string File { get; set; } = "changeLog.yml";

        [Description(Description.AllTypeDescription)]
        [CommandOption("-t|--type")]
        public string? Type { get; set; }

        [Description("Name of object to filter on")]
        [CommandOption("-n|--name")]
        public string? Name { get; set; }

        [Description("Show detailed comparison results")]
        [CommandOption("-d|--detail")]
        public bool Detail { get; set; }
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var config = ConfigHelper.GetConfig(settings.File, settings.ConnectionString, settings.SourceConnectionString);
        Builder builder = new(config);
        if (!builder.Validate())
        {
            return await Task.FromResult(1);
        }

        var target = await AnsiConsole
            .Status()
            .StartAsync("Getting target meta...", _ => Db.GetAllObjectsAndTable(builder, false));

        target = (settings.Type != null) ? target.Where(x => x.Type.Equals(settings.Type)).ToList() : target;

        var source = await AnsiConsole
            .Status()
            .StartAsync("Getting source meta...", _ => Db.GetAllObjectsAndTable(builder, true));

        source = (settings.Type != null) ? source.Where(x => x.Type.Equals(settings.Type)).ToList() : source;

        if (target.Count == 0 && source.Count == 0)
        {
            AnsiConsole.Markup("[bold red]:red_exclamation_mark: No items found in source or target[/] ");
            return await Task.FromResult(0);
        }

        if (settings.Name?.Length > 0)
        {
            target = target.Where(x => x.Name.Equals(settings.Name ?? "")).ToList();
            source = source.Where(x => x.Name.Equals(settings.Name ?? "")).ToList();
            AnsiConsole.MarkupLine($"[bold red]:red_exclamation_mark: Filtering on {settings.Name} [/] ");
        }

        var add = GetDiffItems(target, source);
        var delete = GetDiffItems(source, target);
        var diff = GetDiffDetail(source, target);

        if (add.Count == 0 && delete.Count == 0 && diff.Changed.Count == 0)
        {
            AnsiConsole.Markup("[bold green]:party_popper: Databases in sync.[/] ");
            AnsiConsole.Markup($"[blue]Checked {target.Count} items[/]");
            return await Task.FromResult(0);
        }

        TableMeta(add.ToList(), "[blue]:package: Add items[/]", Color.Blue);
        TableMeta(delete.ToList(), "[red]:bomb: Remove items[/]", Color.Red);
        TableMeta(diff.Changed, "[yellow]:pill: Changed items[/]", Color.Yellow);

        AnsiConsole.WriteLine();

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("Name");
        table.AddColumn("Value");
        table.BorderColor(Color.Yellow);
        table.AddRow("[mediumpurple3]:purple_circle: Target[/]", $"[mediumpurple3]{target.Count}[/]");
        table.AddRow("[orange3]:orange_circle: Source[/]", $"[orange3]{source.Count}[/]");
        table.AddRow("[blue]:package: Add[/]", $"[blue]{add.Count}[/]");
        table.AddRow("[red]:bomb: Delete[/]", $"[red]{delete.Count}[/]");
        table.AddRow("[yellow]:pill: Changed[/]", $"[yellow]{diff.Changed.Count}[/]");
        table.AddRow("[green]:party_popper: Matched[/]", $"[green]{diff.Matched.Count}[/]");
        AnsiConsole.Write(table);

        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[bold yellow]:squid: Found  mismatch in source and target[/] ");

        if (settings.Detail)
        {
            AnsiConsole.WriteLine();
            foreach (var item in diff.Changed)
            {
                var rule = new Rule($"{item.Schema}.{item.Name}");
                rule.LeftJustified();
                rule.RuleStyle("teal dim");
                AnsiConsole.WriteLine();
                AnsiConsole.Write(rule);
                AnsiConsole.WriteLine();

                var diffResults = InlineDiffBuilder.Diff(item.Definition.Trim(), item.Definition2.Trim());
                foreach (var line in diffResults.Lines)
                {
                    switch (line.Type)
                    {
                        case ChangeType.Inserted:
                            AnsiConsole.MarkupLineInterpolated($"[green]+ {line.Text}[/]");
                            break;
                        case ChangeType.Deleted:
                            AnsiConsole.MarkupLineInterpolated($"[red]- {line.Text}[/]");
                            break;
                        default:
                            AnsiConsole.MarkupLineInterpolated($"[gray]  {line.Text}[/]");
                            break;
                    }
                }
            }
        }
        return await Task.FromResult(0);
    }

    public static DiffMetaDTO GetDiffDetail(List<MetaDTO> list1, List<MetaDTO> list2)
    {
        DiffMetaDTO diff = new();
        foreach (var l1 in list1)
        {
            foreach (var l2 in list2)
            {
                if (l1.Schema.Equals(l2.Schema) && l1.Name.Equals(l2.Name))
                {
                    if (Generator.IsMatch(l1, l2))
                    {
                        diff.Matched.Add(l1);
                    }
                    else
                    {
                        diff.Changed.Add(new ChangedDTO
                        {
                            Name = l1.Name,
                            ObjectId = l1.ObjectId,
                            Schema = l1.Schema,
                            Type = l1.Type,
                            TypeName = l1.TypeName,
                            Definition = l1.Definition,
                            Definition2 = l2.Definition
                        });
                    }
                }
            }
        }
        return diff;
    }

    public static List<MetaDTO> GetDiffItems(List<MetaDTO> list1, List<MetaDTO> list2)
    {
        var result = list1.Where(s => !list2.Any(d => d.Name == s.Name));
        return result.ToList();
    }

    public static void TableMeta(List<MetaDTO> meta, string title, Color color)
    {
        if (meta.Count == 0) return;

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(title);
        table.AddColumn("Type");
        table.AddColumn("Type Name");
        table.BorderColor(color);
        foreach (var row in meta)
        {
            table.AddRow(row.Name, row.Type, row.TypeName);
        }
        AnsiConsole.Write(table);
    }

    public static void TableMeta(List<ChangedDTO> meta, string title, Color color)
    {
        if (meta.Count == 0) return;
        var newMeta = meta.ConvertAll(a => new MetaDTO()
        {
            Name = a.Name,
            Definition = a.Definition,
            ObjectId = a.ObjectId,
            Schema = a.Schema,
            Type = a.Type,
            TypeName = a.TypeName
        });
        TableMeta(newMeta, title, color);
    }
}
