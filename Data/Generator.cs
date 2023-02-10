using System.Text;

namespace ChangeLog.Data;

public static class Generator
{
    public static List<MetaDTO> TableDefinition(IEnumerable<MetaDTO> objects, IEnumerable<ColumnsDTO> columns, IEnumerable<ColumnConstraintDTO> constraints, IEnumerable<ForeignKeysDTO> foreignKeys, IEnumerable<IndexDTO> indexes)
    {
        List<Column> newColumns = new();
        foreach (var col in columns)
        {
            Column column = new()
            {
                ColumnId = col.ColumnId,
                ColumnName = col.ColumnName,
                ComputedDefinition = col.ComputedDefinition,
                DefaultConstraint = col.DefaultConstraint,
                DefaultConstraintName = col.DefaultConstraintName,
                IsIdentity = col.IsIdentity,
                IsNullable = col.IsNullable,
                MaxLength = col.MaxLength,
                ObjectId = col.ObjectId,
                Precision = col.Precision,
                Scale = col.Scale,
                Schema = col.Schema,
                TableName = col.TableName,
                TypeId = col.TypeId,
                TypeName = col.TypeName,
                IndexName = null,
            };

            foreach (var pk in constraints)
            {
                if (pk.ObjectId == column.ObjectId &&
                    pk.ColumnId == column.ColumnId &&
                    pk.IsPrimaryKey
                )
                {
                    column.IsPrimaryKey = true;
                }
            }
            newColumns.Add(column);
        }

        foreach (var o in objects)
        {
            if (o.Type == "U")
            {
                o.Definition = TableDefinitionSQL(o, newColumns, indexes);
            }
        }

        return objects.ToList();
    }

    private static string TableDefinitionSQL(MetaDTO o, List<Column> allColumns, IEnumerable<IndexDTO> allIndexes)
    {
        var columns = allColumns
            .Where(x => x.ObjectId.Equals(o.ObjectId))
            .OrderBy(o => o.ColumnId)
            .ToList();

        var indexes = allIndexes
            .Where(x => x.ObjectId.Equals(o.ObjectId))
            .OrderBy(o => o.IndexName)
            .ToList();

        StringBuilder sql = new();
        sql.AppendFormat("CREATE TABLE {0}.{1}\r\n", o.Schema, o.Name);
        sql.AppendFormat("(\r\n");

        List<string> rawColumnTypes = columns.ConvertAll(c => c.GetDefinitionType().ToUpper());
        int maxColumnName = columns.Max(c => c.ColumnName.Length);
        int maxColumnType = rawColumnTypes.Max(c => c.Length);

        List<string> columnNames = columns.ConvertAll(c => c.ColumnName + new string(' ', maxColumnName - c.ColumnName.Length));
        List<string> columnTypes = rawColumnTypes.ConvertAll(c => c + new string(' ', maxColumnType - c.Length));

        for (int c = 0; c < columns.Count; c++)
        {
            Column column = columns[c];

            if (!string.IsNullOrWhiteSpace(column.ComputedDefinition))
            {
                sql.AppendFormat(
                    "    {0} AS {1}{2}\r\n",
                    columnNames[c],
                    column.ComputedDefinition,
                    c == columns.Count - 1 ? "" : ",");
            }
            else
            {
                string colDef = column.GetDefinitionType();
                sql.AppendFormat(
                    "    {0} {1} {2}{3}{4}{5}{6}\r\n",
                    columnNames[c],
                    columnTypes[c],
                    column.IsNullable ? "    NULL" : "NOT NULL",
                    string.IsNullOrWhiteSpace(column.DefaultConstraint) ? "" : " DEFAULT " + column.DefaultConstraint,
                    column.IsPrimaryKey ? " PRIMARY KEY" : "",
                    column.IsIdentity ? " IDENTITY(1,1)" : "",
                    c == columns.Count - 1 ? "" : ",");
            }
        }

        sql.AppendFormat(")\r\n");

        var newIndexes = Index.GetFromDTO(indexes);

        if (newIndexes.Count > 0)
        {
            sql.Append("GO\r\n\r\n");

            foreach (var index in newIndexes.OrderBy(c => c.IndexName))
            {
                sql.AppendFormat("{0}\r\nGO\r\n", index.CreateSQL);
            }
        }
        return sql.ToString();
    }

    public static string DefinitionCleanup(string input)
    {
        var result = input
            .Replace(@"\r", string.Empty)
            .Replace(@"\n", string.Empty)
            .Replace(@"\t", string.Empty);
        return result;
    }
}