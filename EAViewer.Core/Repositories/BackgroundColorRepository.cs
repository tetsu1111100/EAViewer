using Dapper;
using EAViewer.Core.Interfaces;
using EAViewer.Core.Models;
using Microsoft.Data.SqlClient;

namespace EAViewer.Core.Repositories;

public sealed class BackgroundColorRepository : IBackgroundColorRepository
{
    private readonly string _connectionString;

    public BackgroundColorRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<IEnumerable<CellBackgroundColor>> GetBackgroundColorsAsync(
        string dbHost, string dbName, string tableName)
    {
        const string sql = @"
            SELECT DbHost, DbName, TableName, ColName, TargetColumnName, BackgroundColor
            FROM EA_CELL_BACKGROUND_COLOR
            WHERE DbHost = @DbHost AND DbName = @DbName AND TableName = @TableName";

        await using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<CellBackgroundColor>(sql,
            new { DbHost = dbHost, DbName = dbName, TableName = tableName });
    }

    public async Task SaveBackgroundColorsAsync(string dbHost, string dbName, string tableName, IEnumerable<CellBackgroundColor> colors)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            // 1. Clear all existing colors for this table
            const string deleteSql = @"
                DELETE FROM EA_CELL_BACKGROUND_COLOR
                WHERE DbHost = @DbHost AND DbName = @DbName AND TableName = @TableName";

            await connection.ExecuteAsync(deleteSql, new { DbHost = dbHost, DbName = dbName, TableName = tableName }, transaction);

            // 2. Insert new colors if any
            var colorList = colors?.ToList();
            if (colorList != null && colorList.Count > 0)
            {
                const string insertSql = @"
                    INSERT INTO EA_CELL_BACKGROUND_COLOR
                    (DbHost, DbName, TableName, ColName, TargetColumnName, BackgroundColor)
                    VALUES
                    (@DbHost, @DbName, @TableName, @ColName, @TargetColumnName, @BackgroundColor)";

                // Dapper handles IEnumerable seamlessly for bulk inserts
                await connection.ExecuteAsync(insertSql, colorList, transaction);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
