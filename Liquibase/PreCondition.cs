using ChangeLog.Liquibase.ChangeTypes;

namespace ChangeLog.Liquibase;

public class PreCondition
{
    public string? OnFail { get; set; }
    public SqlCheckType? SqlCheck { get; set; }
}