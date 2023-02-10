namespace ChangeLog.Data;

public class Index
{
    public int ObjectId { get; set; }
    public string Schema { get; set; } = "";
    public string IndexName { get; set; } = "";
    public string TableName { get; set; } = "";
    public List<string> ColumnNames { get; set; } = new List<string>();
    public List<string> IncludeColumnNames { get; set; } = new List<string>();

    public static List<Index> GetFromDTO(IEnumerable<IndexDTO> indexDTO)
    {
        List<Index> ret = new();
        foreach (var item in indexDTO)
        {
            Index? index = ret.Find(i => i.ObjectId.Equals(item.ObjectId) && i.IndexName.Equals(item.IndexName, StringComparison.CurrentCultureIgnoreCase));
            if (index == null)
            {
                index = new Index()
                {
                    Schema = item.Schema,
                    ObjectId = item.ObjectId,
                    TableName = item.TableName,
                    IndexName = item.IndexName
                };
                ret.Add(index);
            }

            if (item.IsIncludedColumn)
            {
                index.IncludeColumnNames.Add(item.ColumnName);
            }
            else
            {
                index.ColumnNames.Add(item.ColumnName);
            }
        }

        return ret;
    }

    public string CreateSQL
    {
        get
        {
            return string.Format(
                "CREATE INDEX {1} ON {0}.{2}({3}){4}",
                Schema, IndexName, TableName, string.Join(",", ColumnNames),
                IncludeColumnNames.Count == 0 ? "" : " INCLUDE (" + string.Join(",", IncludeColumnNames) + ")"
                );
        }
    }
}