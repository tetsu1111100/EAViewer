using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EAViewer.Core.Models;
using EAViewer.Core.Services;

namespace EAViewer.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly TableService _tableService;
    private CancellationTokenSource? _noDataCts;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLowerCaseColName;

    [ObservableProperty]
    private bool _isNoDataVisible;

    [ObservableProperty]
    private bool _isSearching;

    public ObservableCollection<TableGridViewModel> TableGrids { get; } = new();

    public MainWindowViewModel(TableService tableService)
    {
        _tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        // Cancel any existing "no data" timer
        if (_noDataCts != null)
        {
            try
            {
                if (!_noDataCts.IsCancellationRequested)
                    _noDataCts.Cancel();
            }
            catch (ObjectDisposedException) { }
            
            _noDataCts.Dispose();
            _noDataCts = null;
        }

        IsNoDataVisible = false;

        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        var searchTerm = SearchText.Trim();
        SearchText = string.Empty; // Fix 1: Clear input after search

        IsSearching = true;

        try
        {
            // Fix 3: Do NOT clear existing grids — each search appends independently
            var tables = await _tableService.SearchTablesAsync(searchTerm);
            var tableList = tables.ToList();

            if (tableList.Count == 0)
            {
                ShowNoDataMessage();
                return;
            }

            // Position variables: offset based on existing grid count
            int existingCount = TableGrids.Count;
            double offsetX = 30;
            double offsetY = 10;
            int index = existingCount;

            foreach (var table in tableList)
            {
                var (details, colors) = await _tableService.GetTableDetailsWithColorsAsync(
                    table.DbHost, table.DbName, table.TableName);

                var gridVm = new TableGridViewModel(table, details, colors, _tableService)
                {
                    IsLowerCaseColName = IsLowerCaseColName,
                    CanvasX = offsetX + (index % 4) * 420,
                    CanvasY = offsetY + (index / 4) * 380
                };

                gridVm.RequestClose += (g) => TableGrids.Remove(g);

                TableGrids.Add(gridVm);
                index++;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"搜尋發生錯誤: {ex.Message}", "錯誤",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void OpenNew()
    {
        var newWindow = new Views.MainWindow(_tableService);
        newWindow.Show();
    }

    partial void OnIsLowerCaseColNameChanged(bool value)
    {
        foreach (var grid in TableGrids)
        {
            grid.IsLowerCaseColName = value;
        }
    }

    private void ShowNoDataMessage()
    {
        _noDataCts = new CancellationTokenSource();
        IsNoDataVisible = true;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(3000, _noDataCts.Token);
                Application.Current?.Dispatcher.Invoke(() => IsNoDataVisible = false);
            }
            catch (TaskCanceledException)
            {
                // Timer was cancelled (new search started)
            }
        });
    }
}
