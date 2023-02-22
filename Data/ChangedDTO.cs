namespace ChangeLog.Data;

public class ChangedDTO
{
    public int ObjectId { get; set; }
    public string Schema { get; set; } = "";
    public string Name { get; set; } = "";
    public string Definition { get; set; } = "";
    public string Definition2 { get; set; } = "";
    public string Type { get; set; } = "";
    public string TypeName { get; set; } = "";
}