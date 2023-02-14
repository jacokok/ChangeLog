using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using ChangeLog.Classes;
using ChangeLog.Liquibase;
using ChangeLog.Liquibase.ChangeTypes;
using ChangeLog.Utils;
using Dapper;
using FluentValidation;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ChangeLog.Command;

public class SeedCommand : AsyncCommand<SeedCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-s|--sourceConnectionString")]
        public string SourceConnectionString { get; set; } = "";

        [CommandOption("-f|--file")]
        public string File { get; set; } = "changeLog.yml";

        [Description("Table Name to get seed data from")]
        [CommandOption("-t|--table")]
        public string? Table { get; set; }
        [Description("Schema")]
        [CommandOption("-d|--schema")]
        public string Schema { get; set; } = "dbo";
        [CommandArgument(1, "[OutputFile]")]
        public string Output { get; set; } = "./seed.yml";
    }

    public class SettingsValidator : AbstractValidator<Settings>
    {
        public SettingsValidator()
        {
            RuleFor(v => v.Table).NotEmpty();
        }
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var config = ConfigHelper.GetConfig(settings.File, null, settings.SourceConnectionString);
        Builder builder = new(config);
        if (!builder.Validate())
        {
            return await Task.FromResult(0);
        }

        if (!Builder.ValidateCustom(new SettingsValidator().Validate(settings)))
        {
            return await Task.FromResult(0);
        }

        var tableResults = await AnsiConsole
            .Status()
            .StartAsync("Getting table data...", _ => GetTableData(builder, settings.Table!, settings.Schema));

        if (tableResults == null || tableResults.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No records found in table: {settings.Table}[/]");
            return await Task.FromResult(0);
        }

        AnsiConsole.MarkupLine($"Generating data count: [green]{tableResults.Count}[/]");

        string path = Path.GetFullPath(settings.Output);

        LiquibaseContainer changeLog = new()
        {
            DatabaseChangeLog = new()
        };
        List<ChangeType> changes = new();
        changes.Add(
            new ChangeType
            {
                Sql = new SqlType
                {
                    Sql = "SET IDENTITY_INSERT NodeType ON;"
                }
            }
        );

        foreach (var row in tableResults.ToArray())
        {
            List<ColumnContainer> columns = new();
            foreach (var col in (IDictionary<string, object>)row)
            {
                columns.Add(new ColumnContainer
                {
                    Column = new Column
                    {
                        Name = col.Key,
                        Value = col.Value?.ToString()
                    }
                });
            }
            changes.Add(new ChangeType
            {
                Insert = new InsertType
                {
                    SchemaName = settings.Schema,
                    TableName = settings.Table!,
                    Columns = columns
                }
            });
        }
        changes.Add(
            new ChangeType
            {
                Sql = new SqlType
                {
                    Sql = "SET IDENTITY_INSERT NodeType ON;"
                }
            }
        );

        changeLog.DatabaseChangeLog.Add(
            new DatabaseChangeLog
            {
                ChangeSet = new()
                {
                    Author = Environment.UserName,
                    Id = "Todo",
                    Changes = changes
                }
            }
        );

        var yaml = Yaml.GetSerializer().Serialize(changeLog);
        File.WriteAllText(path, yaml);

        return await Task.FromResult(1);
    }

    private static async Task<List<dynamic>> GetTableData(Builder builder, string table, string schema)
    {
        using var connection = builder.GetSourceConnection();
        const string sql = @"
            SELECT
                OBJECTPROPERTY(OBJECT_ID(TABLE_NAME), 'IsTable') IsTable,
                OBJECTPROPERTY(OBJECT_ID(TABLE_NAME), 'TableHasIdentity') TableHasIdentity
            FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @table AND TABLE_SCHEMA = @schema
        ";
        var objectPropertyResult = await connection.QuerySingleAsync<Data.ObjectProperty>(sql, new { table, schema });

        if (objectPropertyResult?.IsTable == true)
        {
            return (await connection.QueryAsync($"SELECT * FROM {schema}.{table}")).ToList();
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]Could not find table: {schema}.{table}[/]");
        }
        return new List<dynamic>();
    }
}
