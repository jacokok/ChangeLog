namespace ChangeLog.Liquibase.ChangeTypes;

public class ColumnContainer
{
    public Column? Column { get; set; }
}

public class Column
{
    public string Name { get; set; } = "";
    public string? Value { get; set; }
    public string? Type { get; set; }
}