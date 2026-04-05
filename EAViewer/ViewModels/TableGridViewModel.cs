using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EAViewer.Core.Models;
using EAViewer.Core.Services;

namespace EAViewer.ViewModels;

/// <summary>
/// Represents a single row in the table detail grid with background color support.
/// </summary>
public partial class TableDetailRow : ObservableObject
{
    private readonly TableDetail _detail;

    public TableDetailRow(TableDetail detail)
    {
        _detail = detail ?? throw new ArgumentNullException(nameof(detail));
    }

    public TableDetail Detail => _detail;

    public string ColName => _detail.ColName;
    public string? ColDesc => _detail.ColDesc;
    public string? Remark => _detail.Remark;

    // Display-only property respecting lowercase setting
    [ObservableProperty]
    private string _displayColName = string.Empty;

    [ObservableProperty]
    private Brush? _colNameBackground;

    [ObservableProperty]
    private Brush? _colDescBackground;

    [ObservableProperty]
    private Brush? _remarkBackground;

    [ObservableProperty]
    private bool _isHighlighted;

    /// <summary>
    /// Builds tooltip text for ColName hover.
    /// </summary>
    public string ColNameTooltip
    {
        get
        {
            var lines = new List<string> { _detail.ColName };

            if (!string.IsNullOrWhiteSpace(_detail.ColType))
                lines.Add(_detail.ColType);

            if (_detail.IsPrimaryKey)
                lines.Add("🔑 Primary Key");

            if (_detail.IsAutoIncrement)
                lines.Add("+ AutoIncrement");

            if (!_detail.IsNullAllowed)
                lines.Add("+ 不允許Null");

            if (_detail.HasDefaultValue)
                lines.Add($"+ Default: {_detail.DefaultValue}");

            return string.Join(Environment.NewLine, lines);
        }
    }

    public void UpdateDisplayColName(bool lowerCase)
    {
        DisplayColName = lowerCase ? ColName.ToLowerInvariant() : ColName;
    }
}

/// <summary>
/// ViewModel for a single draggable table grid card on the canvas.
/// </summary>
public partial class TableGridViewModel : ObservableObject
{
    private readonly TableService _tableService;
    private readonly Dictionary<(string ColName, string TargetColumnName), string> _colorMap = new();

    public TableInfo TableInfo { get; }
    public string DisplayTitle => TableInfo.DisplayTitle;

    [ObservableProperty]
    private double _canvasX;

    [ObservableProperty]
    private double _canvasY;

    [ObservableProperty]
    private bool _isLowerCaseColName;

    private static int _globalZIndex = 0;

    [ObservableProperty]
    private int _zIndex;

    public void BringToFront()
    {
        ZIndex = System.Threading.Interlocked.Increment(ref _globalZIndex);
    }

    public ObservableCollection<TableDetailRow> Rows { get; } = new();

    public TableGridViewModel(
        TableInfo tableInfo,
        IEnumerable<TableDetail> details,
        IEnumerable<CellBackgroundColor> colors,
        TableService tableService)
    {
        TableInfo = tableInfo ?? throw new ArgumentNullException(nameof(tableInfo));
        _tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));

        BringToFront();

        // Build color map
        foreach (var c in colors)
        {
            _colorMap[(c.ColName, c.TargetColumnName)] = c.BackgroundColor;
        }

        // Build rows
        foreach (var detail in details)
        {
            var row = new TableDetailRow(detail);
            row.UpdateDisplayColName(IsLowerCaseColName);
            ApplyBackgroundColors(row);
            Rows.Add(row);
        }
    }

    partial void OnIsLowerCaseColNameChanged(bool value)
    {
        foreach (var row in Rows)
        {
            row.UpdateDisplayColName(value);
        }
    }

    public event Action<TableGridViewModel>? RequestClose;

    [RelayCommand]
    private void Close()
    {
        RequestClose?.Invoke(this);
    }

    [RelayCommand]
    private void CopyTableName()
    {
        try
        {
            Clipboard.SetText(TableInfo.TableName);
        }
        catch
        {
            // Clipboard access can fail in some environments
        }
    }

    [RelayCommand]
    private async Task SaveBackgroundColorAsync()
    {
        try
        {
            var colorsToSave = new List<CellBackgroundColor>();

            foreach (var row in Rows)
            {
                AddColorIfSet(colorsToSave, row, row.ColNameBackground, "ColName");
                AddColorIfSet(colorsToSave, row, row.ColDescBackground, "ColDesc");
                AddColorIfSet(colorsToSave, row, row.RemarkBackground, "Remark");
            }

            if (colorsToSave.Count > 0)
            {
                await _tableService.SaveBackgroundColorsAsync(colorsToSave);
                MessageBox.Show("背景色儲存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("沒有需要儲存的背景色設定。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"儲存失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Sets the background color for a specific cell.
    /// </summary>
    public void SetCellColor(TableDetailRow row, string targetColumnName, string rgbColor)
    {
        var brush = ParseRgbBrush(rgbColor);
        _colorMap[(row.ColName, targetColumnName)] = rgbColor;

        switch (targetColumnName)
        {
            case "ColName":
                row.ColNameBackground = brush;
                break;
            case "ColDesc":
                row.ColDescBackground = brush;
                break;
            case "Remark":
                row.RemarkBackground = brush;
                break;
        }
    }

    private void ApplyBackgroundColors(TableDetailRow row)
    {
        if (_colorMap.TryGetValue((row.ColName, "ColName"), out var colNameColor))
            row.ColNameBackground = ParseRgbBrush(colNameColor);

        if (_colorMap.TryGetValue((row.ColName, "ColDesc"), out var colDescColor))
            row.ColDescBackground = ParseRgbBrush(colDescColor);

        if (_colorMap.TryGetValue((row.ColName, "Remark"), out var remarkColor))
            row.RemarkBackground = ParseRgbBrush(remarkColor);
    }

    private void AddColorIfSet(List<CellBackgroundColor> list, TableDetailRow row, Brush? brush, string targetCol)
    {
        if (brush is SolidColorBrush scb)
        {
            list.Add(new CellBackgroundColor
            {
                DbHost = TableInfo.DbHost,
                DbName = TableInfo.DbName,
                TableName = TableInfo.TableName,
                ColName = row.ColName,
                TargetColumnName = targetCol,
                BackgroundColor = $"rgb({scb.Color.R}, {scb.Color.G}, {scb.Color.B})"
            });
        }
    }

    public static SolidColorBrush? ParseRgbBrush(string? rgbString)
    {
        if (string.IsNullOrWhiteSpace(rgbString))
            return null;

        // Parse "rgb(R, G, B)" format
        var trimmed = rgbString.Trim();
        if (trimmed.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(")"))
        {
            var inner = trimmed[4..^1];
            var parts = inner.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length == 3 &&
                byte.TryParse(parts[0], out byte r) &&
                byte.TryParse(parts[1], out byte g) &&
                byte.TryParse(parts[2], out byte b))
            {
                return new SolidColorBrush(Color.FromRgb(r, g, b));
            }
        }

        return null;
    }
}
