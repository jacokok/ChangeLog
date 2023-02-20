namespace ChangeLog.Liquibase.ChangeTypes;

public class ChangeType
{
    public SqlType? Sql { get; set; }
    public CreateProcedureType? CreateProcedure { get; set; }
    public DropProcedureType? DropProcedure { get; set; }
    public CreateViewType? CreateView { get; set; }
    public InsertType? Insert { get; set; }
}