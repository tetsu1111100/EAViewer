namespace EAViewer.Core.Models;

/// <summary>
/// Maps to EA_CELL_BACKGROUND_COLOR table.
/// </summary>
public sealed class CellBackgroundColor
{
    public string DbHost { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ColName { get; set; } = string.Empty;
    public string TargetColumnName { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
}
