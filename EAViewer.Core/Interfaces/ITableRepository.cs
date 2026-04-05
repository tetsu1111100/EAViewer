using EAViewer.Core.Models;

namespace EAViewer.Core.Interfaces;

public interface ITableRepository
{
    /// <summary>
    /// Searches tables by name (fuzzy match) from V_EA_VIEWER_TABLE.
    /// </summary>
    Task<IEnumerable<TableInfo>> SearchTablesAsync(string tableName);

    /// <summary>
    /// Gets column details from V_EA_VIEWER_TABLE_DETAIL for a specific table.
    /// </summary>
    Task<IEnumerable<TableDetail>> GetTableDetailsAsync(string dbHost, string dbName, string tableName);
}
