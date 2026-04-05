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

    public async Task SaveBackgroundColorsAsync(IEnumerable<CellBackgroundColor> colors)
    {
        var colorList = colors?.ToList();
        if (colorList == null || colorList.Count == 0) return;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            foreach (var color in colorList)
            {
                const string checkSql = @"
                    SELECT COUNT(*)
                    FROM EA_CELL_BACKGROUND_COLOR
                    WHERE DbHost = @DbHost AND DbName = @DbName
                      AND TableName = @TableName AND ColName = @ColName
                      AND TargetColumnName = @TargetColumnName";

                bool exists = await connection.QuerySingleAsync<int>(checkSql, new
                {
                    color.DbHost,
                    color.DbName,
                    color.TableName,
                    color.ColName,
                    color.TargetColumnName
                }, transaction) > 0;

                if (exists)
                {
                    const string updateSql = @"
                        UPDATE EA_CELL_BACKGROUND_COLOR
                        SET BackgroundColor = @BackgroundColor
                        WHERE DbHost = @DbHost AND DbName = @DbName
                          AND TableName = @TableName AND ColName = @ColName
                          AND TargetColumnName = @TargetColumnName";

                    await connection.ExecuteAsync(updateSql, new
                    {
                        color.DbHost,
                        color.DbName,
                        color.TableName,
                        color.ColName,
                        color.TargetColumnName,
                        color.BackgroundColor
                    }, transaction);
                }
                else
                {
                    const string insertSql = @"
                        INSERT INTO EA_CELL_BACKGROUND_COLOR
                        (DbHost, DbName, TableName, ColName, TargetColumnName, BackgroundColor)
                        VALUES
                        (@DbHost, @DbName, @TableName, @ColName, @TargetColumnName, @BackgroundColor)";

                    await connection.ExecuteAsync(insertSql, new
                    {
                        color.DbHost,
                        color.DbName,
                        color.TableName,
                        color.ColName,
                        color.TargetColumnName,
                        color.BackgroundColor
                    }, transaction);
                }
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
