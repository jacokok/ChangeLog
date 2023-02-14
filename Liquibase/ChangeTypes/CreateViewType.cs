namespace ChangeLog.Liquibase.ChangeTypes;

public class CreateViewType
{
    public string SelectQuery { get; set; } = "";
    public bool FullDefinition { get; set; }
    public string ViewName { get; set; } = "";
    public string? SchemaName { get; set; }
    public bool ReplaceIfExists { get; set; }
}