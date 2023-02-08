namespace ChangeLog.Liquibase.ChangeTypes;

public class CreateProcedureType
{
    public string ProcedureBody { get; set; } = "";
    public string ProcedureName { get; set; } = "";
    public string? SchemaName { get; set; }
    public bool ReplaceIfExists { get; set; }
}