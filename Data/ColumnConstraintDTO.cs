namespace ChangeLog.Data;

public class ColumnConstraintDTO
{
    public int ObjectId { get; set; }
    public string Schema { get; set; } = "";
    public string ConstraintName { get; set; } = "";
    public int ColumnId { get; set; }
    public string ColumnName { get; set; } = "";
    public string TableName { get; set; } = "";
    public bool IsUnique { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsDescending { get; set; }
}