namespace ChangeLog.Data;

public class ForeignKeysDTO
{
    public int ObjectId { get; set; }
    public string FKName { get; set; } = "";
    public string TableName { get; set; } = "";
    public string FieldName { get; set; } = "";
    public string ReferencesTableName { get; set; } = "";
    public int DeleteReferentialAction { get; set; }
}