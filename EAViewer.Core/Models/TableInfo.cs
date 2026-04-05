namespace EAViewer.Core.Models;

/// <summary>
/// Maps to V_EA_VIEWER_TABLE view.
/// </summary>
public sealed class TableInfo
{
    public string DbHost { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string? TableDesc { get; set; }
    public string? Remark { get; set; }

    /// <summary>
    /// Generates the display title: TableName - TableDesc - [DbHost].[DbName]
    /// </summary>
    public string DisplayTitle 
    {
        get
        {
            if (string.IsNullOrWhiteSpace(TableDesc))
            {
                return $"{TableName} - [{DbHost}].[{DbName}]";
            }
            return $"{TableName} - {TableDesc} - [{DbHost}].[{DbName}]";
        }
    }
}
