namespace ChangeLog.Data;

public class IndexDTO
{
    public int ObjectId { get; set; }
    public string Schema { get; set; } = "";
    public string IndexName { get; set; } = "";
    public string ColumnName { get; set; } = "";
    public int ColumnId { get; set; }
    public string TableName { get; set; } = "";
    public bool IsIncludedColumn { get; set; }
}