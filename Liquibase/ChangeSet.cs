using ChangeLog.Liquibase.ChangeTypes;

namespace ChangeLog.Liquibase;

public class ChangeSet
{
    public string Id { get; set; } = "";
    public string Author { get; set; } = "";
    public string? ContextFilter { get; set; }
    public string? Rollback { get; set; }
    public List<ChangeType>? Changes { get; set; }
    public bool? RunOnChange { get; set; }
}