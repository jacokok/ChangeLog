using System.Data;
using ChangeLog.Utils;
using Microsoft.Data.SqlClient;
using Spectre.Console;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ChangeLog.Classes;

public class Builder
{
    public readonly Config _config;
    public Builder(Config config)
    {
        _config = config;
    }

    public Builder(string filePath)
    {
        string fileText = File.ReadAllText(filePath);
        _config = Yaml.GetDeserializer().Deserialize<Config>(fileText);
    }

    public Builder(string connectionString, string sourceConnectionString)
    {
        _config = (new()
        {
            ConnectionString = connectionString,
            SourceConnectionString = sourceConnectionString
        });
    }

    public bool Validate()
    {
        return ValidateInput();
    }

    public bool ValidateAll()
    {
        if (ValidateInput())
        {
            return HasValidDbConnection();
        }
        else
        {
            return false;
        }
    }

    private bool ValidateInput()
    {
        ConfigValidator validator = new();
        FluentValidation.Results.ValidationResult result = validator.Validate(_config);
        foreach (var error in result.Errors)
        {
            AnsiConsole.MarkupLine($"[red]{error.ErrorMessage}[/]");
        }
        return result.IsValid;
    }

    public static bool ValidateCustom(FluentValidation.Results.ValidationResult validationResult)
    {
        foreach (var error in validationResult.Errors)
        {
            AnsiConsole.MarkupLine($"[red]{error.ErrorMessage}[/]");
        }
        return validationResult.IsValid;
    }

    public IDbConnection GetConnection()
    {
        return new SqlConnection(_config.ConnectionString);
    }
    public IDbConnection GetSourceConnection()
    {
        return new SqlConnection(_config.SourceConnectionString);
    }

    private bool Connect()
    {
        var connection = new SqlConnection(_config.ConnectionString);
        connection.Open();
        var result = connection.State == ConnectionState.Open;
        if (result)
            connection.Close();
        return result;
    }

    private bool HasValidDbConnection()
    {
        bool result = false;
        AnsiConsole.Status()
            .Start("Connecting to db...", ctx =>
            {
                bool isConnected = Connect();
                ctx.Status("Done checking db connection");

                if (isConnected)
                {
                    AnsiConsole.MarkupLine("[green]:check_mark:[/] Connected to db");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]:cross_mark:[/] Could not connect to db");
                }
                result = isConnected;
            });
        return result;
    }
}