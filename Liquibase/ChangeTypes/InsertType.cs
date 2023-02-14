namespace ChangeLog.Liquibase.ChangeTypes;

public class InsertType
{
    public string? SchemaName { get; set; }
    public string TableName { get; set; } = "";
    public List<ColumnContainer>? Columns { get; set; }
}