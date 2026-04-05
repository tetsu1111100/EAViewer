using EAViewer.Core.Interfaces;
using EAViewer.Core.Models;

namespace EAViewer.Core.Services;

/// <summary>
/// Orchestrates table search and detail retrieval with background color enrichment.
/// </summary>
public sealed class TableService
{
    private readonly ITableRepository _tableRepository;
    private readonly IBackgroundColorRepository _backgroundColorRepository;

    public TableService(ITableRepository tableRepository, IBackgroundColorRepository backgroundColorRepository)
    {
        _tableRepository = tableRepository ?? throw new ArgumentNullException(nameof(tableRepository));
        _backgroundColorRepository = backgroundColorRepository ?? throw new ArgumentNullException(nameof(backgroundColorRepository));
    }

    /// <summary>
    /// Searches tables by name and returns matching TableInfo records.
    /// </summary>
    public async Task<IEnumerable<TableInfo>> SearchTablesAsync(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return Enumerable.Empty<TableInfo>();

        return await _tableRepository.SearchTablesAsync(tableName.Trim());
    }

    /// <summary>
    /// Gets table details with pre-loaded background colors.
    /// </summary>
    public async Task<(IEnumerable<TableDetail> Details, IEnumerable<CellBackgroundColor> Colors)>
        GetTableDetailsWithColorsAsync(string dbHost, string dbName, string tableName)
    {
        var detailsTask = _tableRepository.GetTableDetailsAsync(dbHost, dbName, tableName);
        var colorsTask = _backgroundColorRepository.GetBackgroundColorsAsync(dbHost, dbName, tableName);

        await Task.WhenAll(detailsTask, colorsTask);

        return (detailsTask.Result, colorsTask.Result);
    }

    /// <summary>
    /// Saves background colors for a table grid.
    /// </summary>
    public async Task SaveBackgroundColorsAsync(IEnumerable<CellBackgroundColor> colors)
    {
        await _backgroundColorRepository.SaveBackgroundColorsAsync(colors);
    }
}
