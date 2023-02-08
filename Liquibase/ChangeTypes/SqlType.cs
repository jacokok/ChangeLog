namespace ChangeLog.Liquibase.ChangeTypes;

public class SqlType
{
    public string Sql { get; set; } = "";
    public string? Comment { get; set; }
    public bool? SplitStatements { get; set; }
    public bool? StripComments { get; set; }
    public string? EndDelimiter { get; set; }
}