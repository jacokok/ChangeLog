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

        var add = GetDiffItems(destination, source);
        var delete = GetDiffItems(source, destination);
        var changed = GetDiffDetail(source, destination);

        TableMeta(add.ToList(), "[blue]:package: Add items[/]", Color.Blue);
        TableMeta(delete.ToList(), "[red]:bomb: Remove items[/]", Color.Red);
        TableMeta(changed.ToList(), "[yellow]:balance_scale: Changed items[/]", Color.Yellow);

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new BarChart()
            .Width(60)
            .Label("[green bold underline]Number of items[/]")
            .CenterLabel()
            .AddItem("Destination", destination.Count, Color.Blue)
            .AddItem("Source", source.Count, Color.Green));

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new BreakdownChart()
            .Width(60)
            .AddItem("Add", add.Count, Color.Blue)
            .AddItem("Delete", delete.Count, Color.Red)
            .AddItem("Changed", changed.Count, Color.Yellow));

        return await Task.FromResult(1);
    }

    public static List<Data.MetaDTO> GetDiffDetail(List<Data.MetaDTO> list1, List<Data.MetaDTO> list2)
    {
        List<Data.MetaDTO> results = new();
        foreach (var l1 in list1)
        {
            foreach (var l2 in list2)
            {
                if (l1.Schema.Equals(l2.Schema) &&
                    l1.Name.Equals(l2.Name) &&
                    Generator.DefinitionCleanup(l1.Definition) != Generator.DefinitionCleanup(l2.Definition))
                {
                    results.Add(l1);
                }
            }
        }
        return results;
    }

    public static List<Data.MetaDTO> GetDiffItems(List<Data.MetaDTO> list1, List<Data.MetaDTO> list2)
    {
        var result = list1.Where(s => !list2.Any(d => d.Name == s.Name));
        return result.ToList();
    }

    public static void TableMeta(List<Data.MetaDTO> meta, string title, Color color)
    {
        // AnsiConsole.Markup(title);
        // AnsiConsole.WriteLine();

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
