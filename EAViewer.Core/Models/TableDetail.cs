namespace EAViewer.Core.Models;

/// <summary>
/// Maps to V_EA_VIEWER_TABLE_DETAIL view.
/// </summary>
public sealed class TableDetail
{
    public string DbHost { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ColName { get; set; } = string.Empty;
    public string? ColDesc { get; set; }
    public string? Remark { get; set; }
    public string? PK { get; set; }
    public string? AntoIncrement { get; set; }
    public string? ColType { get; set; }
    public string? AllowNull { get; set; }
    public string? DefaultValue { get; set; }
    public string? SortValue { get; set; }

    public bool IsPrimaryKey => string.Equals(PK, "Y", StringComparison.OrdinalIgnoreCase);
    public bool IsAutoIncrement => string.Equals(AntoIncrement, "Y", StringComparison.OrdinalIgnoreCase);
    public bool IsNullAllowed => string.Equals(AllowNull, "Y", StringComparison.OrdinalIgnoreCase);
    public bool HasDefaultValue => !string.IsNullOrWhiteSpace(DefaultValue);
}
