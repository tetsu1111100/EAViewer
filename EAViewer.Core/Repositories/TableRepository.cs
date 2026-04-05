using Dapper;
using EAViewer.Core.Interfaces;
using EAViewer.Core.Models;
using Microsoft.Data.SqlClient;

namespace EAViewer.Core.Repositories;

public sealed class TableRepository : ITableRepository
{
    private readonly string _connectionString;

    public TableRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<IEnumerable<TableInfo>> SearchTablesAsync(string tableName)
    {
        const string sql = @"
            SELECT DbHost, DbName, TableName, TableDesc, Remark
            FROM V_EA_VIEWER_TABLE
            WHERE TableName LIKE @TableName
            ORDER BY DbHost, DbName, TableName";

        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<TableInfo>(sql, new { TableName = $"%{tableName}%" });
    }

    public async Task<IEnumerable<TableDetail>> GetTableDetailsAsync(string dbHost, string dbName, string tableName)
    {
        const string sql = @"
            SELECT DbHost, DbName, TableName, ColName, ColDesc, Remark,
                   PK, AntoIncrement, ColType, AllowNull, DefaultValue, SortValue
            FROM V_EA_VIEWER_TABLE_DETAIL
            WHERE DbHost = @DbHost AND DbName = @DbName AND TableName = @TableName
            ORDER BY SortValue";

        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<TableDetail>(sql, new { DbHost = dbHost, DbName = dbName, TableName = tableName });
    }
}
