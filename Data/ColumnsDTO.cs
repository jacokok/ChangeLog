namespace ChangeLog.Data;

public class ColumnsDTO
{
    public int ObjectId { get; set; }
    public string Schema { get; set; } = "";
    public string TableName { get; set; } = "";
    public int ColumnId { get; set; }
    public string ColumnName { get; set; } = "";
    public int TypeId { get; set; }
    public string TypeName { get; set; } = "";
    public int Precision { get; set; }
    public int Scale { get; set; }
    public int MaxLength { get; set; }
    public bool IsNullable { get; set; }
    public bool IsIdentity { get; set; }
    public string? DefaultConstraint { get; set; }
    public string? DefaultConstraintName { get; set; }
    public string? ComputedDefinition { get; set; }
}