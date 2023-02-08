namespace ChangeLog.Liquibase.ChangeTypes;

public class ChangeType
{
    public SqlType? Sql { get; set; }
    public CreateProcedureType? CreateProcedure { get; set; }
}