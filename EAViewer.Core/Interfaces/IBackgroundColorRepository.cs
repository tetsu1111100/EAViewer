using EAViewer.Core.Models;

namespace EAViewer.Core.Interfaces;

public interface IBackgroundColorRepository
{
    /// <summary>
    /// Gets all saved background colors for a specific table.
    /// </summary>
    Task<IEnumerable<CellBackgroundColor>> GetBackgroundColorsAsync(string dbHost, string dbName, string tableName);

    /// <summary>
    /// Saves (replaces) a collection of background colors in a single transaction.
    /// All existing colors for the table will be cleared before parsing the new ones.
    /// </summary>
    Task SaveBackgroundColorsAsync(string dbHost, string dbName, string tableName, IEnumerable<CellBackgroundColor> colors);
}
