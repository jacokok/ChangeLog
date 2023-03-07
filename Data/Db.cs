using ChangeLog.Classes;
using Dapper;

namespace ChangeLog.Data;

public static class Db
{
    public static async Task<List<MetaDTO>> GetAllObjects(Builder builder, bool isSource)
    {
        using var connection = isSource ? builder.GetSourceConnection() : builder.GetConnection();
        var results = await connection.QueryAsync<MetaDTO>(Queries.GetAllObjectsQuery(), commandTimeout: 300);
        return results.ToList();
    }

    public static async Task<List<MetaDTO>> GetMeta(Builder builder, bool isSource)
    {
        using var connection = isSource ? builder.GetSourceConnection() : builder.GetConnection();
        var results = await connection.QueryAsync<MetaDTO>(Queries.GetMetaQuery(), commandTimeout: 300);
        return results.ToList();
    }

    public static async Task<List<ColumnsDTO>> GetColumns(Builder builder, bool isSource)
    {
        using var connection = isSource ? builder.GetSourceConnection() : builder.GetConnection();
        var results = await connection.QueryAsync<ColumnsDTO>(Queries.GetColumnsQuery(), commandTimeout: 300);
        return results.ToList();
    }

    public static async Task<List<ColumnConstraintDTO>> GetConstraints(Builder builder, bool isSource)
    {
        using var connection = isSource ? builder.GetSourceConnection() : builder.GetConnection();
        var results = await connection.QueryAsync<ColumnConstraintDTO>(Queries.GetConstrainsQuery(), commandTimeout: 300);
        return results.ToList();
    }

    public static async Task<List<ForeignKeysDTO>> GetForeignKeys(Builder builder, bool isSource)
    {
        using var connection = isSource ? builder.GetSourceConnection() : builder.GetConnection();
        var results = await connection.QueryAsync<ForeignKeysDTO>(Queries.GetForeignKeyQuery(), commandTimeout: 300);
        return results.ToList();
    }

    public static async Task<List<IndexDTO>> GetIndexes(Builder builder, bool isSource)
    {
        using var connection = isSource ? builder.GetSourceConnection() : builder.GetConnection();
        var results = await connection.QueryAsync<IndexDTO>(Queries.GetIndexesQuery(), commandTimeout: 300);
        return results.ToList();
    }

    public static async Task<List<MetaDTO>> GetAllObjectsAndTable(Builder builder, bool isSource)
    {
        using var connection = isSource ? builder.GetSourceConnection() : builder.GetConnection();
        var results = await connection.QueryAsync<MetaDTO>(Queries.GetAllObjectsQuery(), commandTimeout: 300);
        var columns = await connection.QueryAsync<ColumnsDTO>(Queries.GetColumnsQuery(), commandTimeout: 300);
        var constraints = await connection.QueryAsync<ColumnConstraintDTO>(Queries.GetConstrainsQuery(), commandTimeout: 300);
        var foreignKeys = await connection.QueryAsync<ForeignKeysDTO>(Queries.GetForeignKeyQuery(), commandTimeout: 300);
        var indexes = await connection.QueryAsync<IndexDTO>(Queries.GetIndexesQuery(), commandTimeout: 300);
        return Generator.TableDefinition(results, columns, constraints, foreignKeys, indexes);
    }
}