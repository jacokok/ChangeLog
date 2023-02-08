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

public class TestCommand : AsyncCommand<TestCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        ChangeSet changeSet1 = new()
        {
            Author = "test",
            Id = "testId"
        };
        List<ChangeType> changes = new()
        {
            new ChangeType
            {
                Sql = new SqlType
                {
                    Comment = "test",
                    EndDelimiter = "GO",
                    SplitStatements = false,
                    StripComments = false,
                    Sql = "SELECT * FROM Test"
                }
            },
            new ChangeType
            {
                CreateProcedure = new CreateProcedureType
                {
                    ProcedureName = "test",
                    ProcedureBody = "test",
                    ReplaceIfExists = true
                }
            }
        };
        ChangeSet changeSet2 = new()
        {
            Author = "test2",
            Id = "testId2",
            Changes = changes
        };
        List<ChangeSet> changeSets = new()
        {
            changeSet1,
            changeSet2
        };
        DatabaseChangeLog databaseChangeLog = new()
        {
            ChangeSet = changeSet1
        };
        DatabaseChangeLog databaseChangeLog2 = new()
        {
            ChangeSet = changeSet2
        };
        List<DatabaseChangeLog> changeLogs = new()
        {
            databaseChangeLog,
            databaseChangeLog2
        };

        LiquibaseContainer container = new()
        {
            DatabaseChangeLog = changeLogs
        };

        var yaml = Yaml.GetSerializer().Serialize(container);

        File.WriteAllText("./output.yaml", yaml);
        return await Task.FromResult(1);
    }
}
