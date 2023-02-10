namespace ChangeLog.Data;

public static class Queries
{
    public static string GetMetaQuery()
    {
        return @"
                SELECT 
                    o.object_id [ObjectId],
                    USER_NAME(o.schema_id) [Schema], 
                    o.name [Name], 
                    TRIM(OBJECT_DEFINITION(o.object_id)) [Definition],
                    TRIM(o.type) [Type],
                    o.type_desc [TypeName]
                FROM  
                    SYS.OBJECTS (NOLOCK) o
                    WHERE 
                        o.is_ms_shipped = 0
                        AND o.type IN ('P', 'FN', 'IF', 'TF')
                        AND NOT (o.name LIKE 'fn_%' AND TRIM(o.type) = 'FN')
                        AND NOT (o.name LIKE 'sp_%' AND TRIM(o.type) = 'P') 
                ORDER BY o.object_id
            ";
    }

    public static string GetAllObjectsQuery()
    {
        return @"
                SELECT 
                    o.object_id [ObjectId],
                    USER_NAME(o.schema_id) [Schema], 
                    o.name [Name], 
                    TRIM(OBJECT_DEFINITION(o.object_id)) [Definition],
                    TRIM(o.type) [Type],
                    o.type_desc [TypeName]
                FROM  
                    SYS.OBJECTS (NOLOCK) o
                    WHERE 
                        o.is_ms_shipped = 0
                        AND o.type IN ('U', 'V', 'P', 'FN', 'IF', 'TF')
                        AND NOT (o.name = 'dtproperties' AND TRIM(o.type) = 'U') 
                        AND NOT (o.name = 'sysdiagrams' AND TRIM(o.type) = 'U') 
                        AND NOT (o.name LIKE 'fn_%' AND TRIM(o.type) = 'FN')
                        AND NOT (o.name LIKE 'sp_%' AND TRIM(o.type) = 'P') 
                ORDER BY o.type
            ";
    }

    public static string GetColumnsQuery()
    {
        return @"
            SELECT 
                USER_NAME(o.schema_id) AS [Schema],
                o.name [TableName],
                o.object_id [ObjectId],
                c.column_id [ColumnId],
                c.name [ColumnName],
                c.user_type_id [TypeId],
                t.name [TypeName],
                c.precision [Precision],
                c.scale [Scale],
                c.max_length [MaxLength],
                c.is_nullable IsNullable,
                c.is_identity IsIdentity,
                TRIM(d.definition) DefaultConstraint,
                d.name DefaultConstraintName,
                TRIM(cc.definition) ComputedDefinition
            FROM sys.objects (NOLOCK) o
                JOIN sys.columns  (NOLOCK) c
                    ON c.object_id = o.object_id
                JOIN sys.types (NOLOCK) t
                    ON t.user_type_id = c.user_type_id
                LEFT JOIN sys.default_constraints (NOLOCK) d
                    ON d.object_id = c.default_object_id
                LEFT JOIN sys.computed_columns (NOLOCK) cc
                    ON cc.object_id = c.object_id
                        AND cc.column_id = c.column_id
            WHERE o.is_ms_shipped = 0
            ORDER BY USER_NAME(o.schema_id), o.name ASC
        ";
    }

    public static string GetConstrainsQuery()
    {
        return @"
                SELECT
                    o.object_id [ObjectId],
                    k.name ConstraintName, 
                    i.is_unique IsUnique, 
                    i.is_primary_key IsPrimaryKey, 
                    ic.is_descending_key IsDescending, 
                    c.column_id ColumnId,
                    c.name ColumnName, 
                    o.name TableName, 
                    s.name [Schema]
                FROM
                    sys.key_constraints (NOLOCK) k
                    JOIN sys.objects (NOLOCK) o 
                        ON o.object_id = k.parent_object_id
                    JOIN sys.schemas (NOLOCK) s 
                        ON o.schema_id = s.schema_id
                    JOIN sys.indexes (NOLOCK) i 
                        ON i.object_id = k.parent_object_id
                    JOIN sys.index_columns (NOLOCK) ic 
                        ON ic.object_id = i.object_id 
                        AND ic.index_id = i.index_id
                    JOIN sys.columns (NOLOCK) c 
                        ON c.object_id = i.object_id 
                        AND c.column_id = ic.column_id
                WHERE i.is_primary_key = 1
                ORDER BY s.name, o.name, c.name
            ";
    }

    public static string GetForeignKeyQuery()
    {
        return @"
                SELECT
                    o.object_id [ObjectId],
                    o.name FKName, 
                    p.name TableName, 
                    c.name FieldName, 
                    ro.name ReferencesTableName, 
                    rc.name ReferencedFieldName, 
                    fk.delete_referential_action DeleteReferentialAction
                FROM 
                    sys.foreign_keys (NOLOCK) fk
                    JOIN  sys.objects (NOLOCK) o
                        ON o.object_id = fk.object_id
                    JOIN  sys.objects (NOLOCK) p
                        ON p.object_id = fk.parent_object_id
                    JOIN  sys.foreign_key_columns (NOLOCK) fkc
                        ON fkc.constraint_object_id = fk.object_id 
                        AND fkc.parent_object_id = fk.parent_object_id
                    JOIN  sys.columns (NOLOCK) c
                        ON c.object_id = fk.parent_object_id 
                        AND c.column_id = fkc.parent_column_id
                    JOIN  sys.objects (NOLOCK) ro 
                        ON ro.object_id = fkc.referenced_object_id
                    JOIN  sys.columns (NOLOCK) rc
                        ON rc.object_id = fkc.referenced_object_id 
                        AND rc.column_id = fkc.referenced_column_id 
                WHERE fk.type = 'F'
                ORDER BY p.name, c.name
            ";
    }

    public static string GetIndexesQuery()
    {
        return @"
            SELECT
				DISTINCT o.object_id [ObjectId],
                s.name [Schema], 
                i.name IndexName, 
                c.name ColumnName,
                c.column_id [ColumnId], 
                o.name TableName, 
                ic.is_included_column IsIncludedColumn
            FROM
                sys.indexes i
                JOIN sys.index_columns ic
                    ON ic.object_id = i.object_id 
                    AND ic.index_id = i.index_id
                JOIN sys.objects o 
                    ON o.object_id = i.object_id 
                    AND o.is_ms_shipped = 0
                JOIN sys.schemas s 
                    ON o.schema_id = s.schema_id
                JOIN sys.columns c 
                    ON c.object_id = o.object_id 
                    AND c.column_id = ic.column_id
            WHERE i.is_primary_key = 0
            ORDER BY o.name, i.name, c.name
            ";
    }
}