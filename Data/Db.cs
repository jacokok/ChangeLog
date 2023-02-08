using ChangeLog.Classes;
using Dapper;

namespace ChangeLog.Data;

public static class Db
{
    private static string GetMetaQuery()
    {
        return @"
                SELECT 
                    ROUTINE_SCHEMA [Schema],
                    SPECIFIC_NAME [Name],
                    OBJECT_DEFINITION(o.[object_id]) [Definition],
                    TRIM(o.type) [Type]
                FROM SYS.OBJECTS (NOLOCK) o
                JOIN INFORMATION_SCHEMA.ROUTINES (NOLOCK) r
                    ON o.[name] = r.ROUTINE_NAME
                WHERE o.type IN ('P', 'FN', 'IF', 'TF')
                AND SPECIFIC_NAME NOT LIKE 'sp_MS%'
                ORDER BY o.object_id
            ";
    }

    public static async Task<List<MetaDTO>> GetMeta(Builder builder)
    {
        using var connection = builder.GetConnection();
        var results = await connection.QueryAsync<MetaDTO>(GetMetaQuery());
        return results.ToList();
    }

    public static async Task<List<MetaDTO>> GetSourceMeta(Builder builder)
    {
        using var connection = builder.GetSourceConnection();
        var results = await connection.QueryAsync<MetaDTO>(GetMetaQuery());
        return results.ToList();
    }
}