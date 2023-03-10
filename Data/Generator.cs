using System.Text;
using System.Text.RegularExpressions;

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
        sql.AppendFormat("CREATE TABLE {0}.{1}", o.Schema, o.Name);
        sql.AppendLine(string.Empty);
        sql.AppendLine(string.Empty);

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
                    "    {0} AS {1}{2}",
                    columnNames[c],
                    column.ComputedDefinition,
                    c == columns.Count - 1 ? "" : ","
                );
                sql.AppendLine(string.Empty);
            }
            else
            {
                string colDef = column.GetDefinitionType();
                sql.AppendFormat(
                    "    {0} {1} {2}{3}{4}{5}{6}",
                    columnNames[c],
                    columnTypes[c],
                    column.IsNullable ? "    NULL" : "NOT NULL",
                    string.IsNullOrWhiteSpace(column.DefaultConstraint) ? "" : " DEFAULT " + column.DefaultConstraint,
                    column.IsPrimaryKey ? " PRIMARY KEY" : "",
                    column.IsIdentity ? " IDENTITY(1,1)" : "",
                    c == columns.Count - 1 ? "" : ",");
                sql.AppendLine(string.Empty);
            }
        }

        sql.AppendLine(")");

        var newIndexes = Index.GetFromDTO(indexes);

        if (newIndexes.Count > 0)
        {
            sql.AppendLine("GO");
            sql.AppendLine(string.Empty);

            foreach (var index in newIndexes.OrderBy(c => c.IndexName))
            {
                sql.Append(index.CreateSQL);
                sql.AppendLine(string.Empty);
                sql.Append("GO");
                sql.AppendLine(string.Empty);
            }
        }
        return sql.ToString();
    }

    public static string DefinitionCleanup(string input)
    {
        input = input.Trim();
        return Regex.Replace(input, @"[\s]", string.Empty);
    }

    public static string DefinitionCustomCleanup(string input, string schema, string name)
    {
        input = input.Trim();
        Match match = Regex.Match(input, @"((?i)\bcreate|alter\b)+[\s]+((?i)\bprocedure|proc|view|table|function\b)+[\s]+([^\s]+)");
        if (match.Groups.Count > 3)
        {
            input = input.Remove(match.Groups[3].Index, match.Groups[3].Length).Insert(match.Groups[3].Index, $"[{schema}].[{name}]");
        }
        return Regex.Replace(input, @"[\s]", " ");
    }

    public static bool IsMatch(MetaDTO item1, MetaDTO item2)
    {
        string def1 = DefinitionCustomCleanup(item1.Definition, item1.Schema, item1.Name);
        string def2 = DefinitionCustomCleanup(item2.Definition, item2.Schema, item2.Name);
        return DefinitionCleanup(def1).Equals(DefinitionCleanup(def2), StringComparison.OrdinalIgnoreCase);
    }
}