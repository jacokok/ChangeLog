using ChangeLog.Liquibase;

namespace ChangeLog.Data;

public class UpdatedMetaDTO
{
    public string Key { get; set; } = "";
    public MetaDTO? Meta { get; set; }
    public LiquibaseContainer? Proc { get; set; }
    public ActionType ActionType { get; set; }
}

public enum ActionType
{
    Add,
    Update,
    Match,
    Delete
}

public class Counts
{
    public int Add { get; set; }
    public int Update { get; set; }
    public int Delete { get; set; }
}