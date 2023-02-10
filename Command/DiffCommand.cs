using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using ChangeLog.Classes;
using ChangeLog.Data;
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

        [Description("Only compare specific types. U = Table, V = View, P = Stored Procedure, FN = Function, IF = Inline Table Function, TF = Table Value Function")]
        [CommandOption("-t|--type")]
        public string? Type { get; set; }
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var config = ConfigHelper.GetConfig(settings.File, settings.ConnectionString, settings.SourceConnectionString);
        Builder builder = new(config);
        if (!builder.Validate())
        {
            return await Task.FromResult(0);
        }

        var destination = await AnsiConsole
            .Status()
            .StartAsync("Getting destination meta...", _ => Data.Db.GetAllObjectsAndTable(builder, false));

        destination = (settings.Type != null) ? destination.Where(x => x.Type.Equals(settings.Type)).ToList() : destination;

        var source = await AnsiConsole
            .Status()
            .StartAsync("Getting source meta...", _ => Data.Db.GetAllObjectsAndTable(builder, true));

        source = (settings.Type != null) ? source.Where(x => x.Type.Equals(settings.Type)).ToList() : source;

        if (destination.Count == 0 && source.Count == 0)
        {
            AnsiConsole.Markup("[bold red]:red_exclamation_mark: No items found in source or destination[/] ");
            return await Task.FromResult(0);
        }

        var add = GetDiffItems(destination, source);
        var delete = GetDiffItems(source, destination);
        var diffTuple = GetDiffDetail(source, destination);
        var changed = diffTuple.Item1;
        var matched = diffTuple.Item2;

        if (add.Count == 0 && delete.Count == 0 && changed.Count == 0)
        {
            AnsiConsole.Markup($"[bold green]:party_popper: Databases in sync.[/] ");
            AnsiConsole.Markup($"[blue]Checked {destination.Count} items[/]");
            return await Task.FromResult(1);
        }

        TableMeta(add.ToList(), "[blue]:package: Add items[/]", Color.Blue);
        TableMeta(delete.ToList(), "[red]:bomb: Remove items[/]", Color.Red);
        TableMeta(changed.ToList(), "[yellow]:pill: Changed items[/]", Color.Yellow);

        AnsiConsole.WriteLine();

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("Name");
        table.AddColumn("Value");
        table.BorderColor(Color.Yellow);
        table.AddRow("[mediumpurple3]:purple_circle: Destination[/]", $"[mediumpurple3]{destination.Count}[/]");
        table.AddRow("[orange3]:orange_circle: Source[/]", $"[orange3]{source.Count}[/]");
        table.AddRow("[blue]:package: Add[/]", $"[blue]{add.Count}[/]");
        table.AddRow("[red]:bomb: Delete[/]", $"[red]{delete.Count}[/]");
        table.AddRow("[yellow]:pill: Changed[/]", $"[yellow]{changed.Count}[/]");
        table.AddRow("[green]:party_popper: Matched[/]", $"[green]{matched.Count}[/]");
        AnsiConsole.Write(table);

        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[bold yellow]:squid: Found  mismatch in source and destination[/] ");
        return await Task.FromResult(0);
    }

    public static Tuple<List<MetaDTO>, List<MetaDTO>> GetDiffDetail(List<Data.MetaDTO> list1, List<Data.MetaDTO> list2)
    {
        List<MetaDTO> changed = new();
        List<MetaDTO> matched = new();
        foreach (var l1 in list1)
        {
            foreach (var l2 in list2)
            {
                if (l1.Schema.Equals(l2.Schema) && l1.Name.Equals(l2.Name))
                {
                    if (Generator.DefinitionCleanup(l1.Definition) != Generator.DefinitionCleanup(l2.Definition))
                    {
                        changed.Add(l1);
                    }
                    else
                    {
                        matched.Add(l1);
                    }
                }
            }
        }
        return Tuple.Create(changed, matched);
    }

    public static List<MetaDTO> GetDiffItems(List<Data.MetaDTO> list1, List<Data.MetaDTO> list2)
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
}
