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
    /// Generates the display title: [DbHost].[DbName].[TableName] TableDesc
    /// </summary>
    public string DisplayTitle =>
        $"[{DbHost}].[{DbName}].[{TableName}] {TableDesc ?? string.Empty}".TrimEnd();
}
