namespace ChangeLog.Data;

public class Column
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
    public bool IsPrimaryKey { get; set; }
    public string? IndexName { get; set; }

    public string GetDefinitionType()
    {
        if (TypeName == "decimal" || TypeName == "numeric")
        {
            return string.Format("{0}({1},{2})", TypeName, Precision, Scale);
        }
        else if (TypeName == "varbinary" || TypeName == "varchar" || TypeName == "binary" || TypeName == "char")
        {
            return string.Format("{0}({1})", TypeName, MaxLength == -1 ? "MAX" : MaxLength.ToString());
        }
        else if (TypeName == "nvarchar" || TypeName == "nchar")
        {
            return string.Format("{0}({1})", TypeName, MaxLength == -1 ? "MAX" : (MaxLength / 2).ToString());
        }
        else
        {
            return TypeName;
        }
    }
}