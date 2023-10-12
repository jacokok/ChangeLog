namespace ChangeLog.Liquibase.ChangeTypes;

public class SqlCheckType
{
    public string ExpectedResult { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
}