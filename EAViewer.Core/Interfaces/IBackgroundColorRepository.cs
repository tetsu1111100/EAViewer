using EAViewer.Core.Models;

namespace EAViewer.Core.Interfaces;

public interface IBackgroundColorRepository
{
    /// <summary>
    /// Gets all saved background colors for a specific table.
    /// </summary>
    Task<IEnumerable<CellBackgroundColor>> GetBackgroundColorsAsync(string dbHost, string dbName, string tableName);

    /// <summary>
    /// Saves (upserts) a collection of background colors in a single transaction.
    /// </summary>
    Task SaveBackgroundColorsAsync(IEnumerable<CellBackgroundColor> colors);
}
