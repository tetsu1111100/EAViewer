using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EAViewer.ViewModels;

namespace EAViewer.Views;

public partial class ColumnSearchWindow : Window
{
    private readonly TableGridViewModel _gridViewModel;
    private readonly string _targetProperty;
    private readonly DataGrid _dataGrid;

    public ColumnSearchWindow(
        TableGridViewModel gridViewModel,
        string targetProperty,
        string columnDisplayName,
        DataGrid dataGrid)
    {
        InitializeComponent();
        _gridViewModel = gridViewModel;
        _targetProperty = targetProperty;
        _dataGrid = dataGrid;

        ColumnLabel.Text = $"搜尋欄位 : 「 {columnDisplayName} 」";
        SearchInput.Focus();
    }

    private void SearchInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            PerformSearch();
        }
        else if (e.Key == Key.Escape)
        {
            SafeClose();
        }
    }

    private bool _isClosing = false;

    private void SafeClose()
    {
        if (!_isClosing)
        {
            _isClosing = true;
            // Explicitly activate owner to prevent focus jumping to other apps
            Owner?.Activate();
            Close();
        }
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        PerformSearch();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        SafeClose();
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        // Close window if clicked outside of it
        SafeClose();
    }

    private void PerformSearch()
    {
        var keyword = SearchInput.Text?.Trim();
        if (string.IsNullOrEmpty(keyword)) return;

        // Clear previous highlights and selection
        _dataGrid.UnselectAll();
        foreach (var row in _gridViewModel.Rows)
        {
            row.IsHighlighted = false;
        }

        TableDetailRow? lastMatch = null;

        foreach (var row in _gridViewModel.Rows)
        {
            string value = _targetProperty switch
            {
                "ColName" => row.DisplayColName ?? string.Empty,
                "ColDesc" => row.ColDesc ?? string.Empty,
                "Remark" => row.Remark ?? string.Empty,
                _ => string.Empty
            };

            if (value.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                row.IsHighlighted = true;
                _dataGrid.SelectedItems.Add(row);
                lastMatch = row;
            }
        }

        // Scroll to last match
        if (lastMatch != null)
        {
            _dataGrid.ScrollIntoView(lastMatch);
        }

        // Auto-close search window after completing the search
        SafeClose();
    }
}
