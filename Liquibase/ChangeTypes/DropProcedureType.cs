namespace ChangeLog.Liquibase.ChangeTypes;

public class DropProcedureType
{
    public string ProcedureName { get; set; } = "";
    public string? SchemaName { get; set; }
}